using Entities;
using Enums;
using Microsoft.AspNetCore.Identity;
using RepositoryContracts;

namespace DevelopmentData;

public sealed class DevelopmentDataSeeder(
    IUserRepository userRepository,
    IPostRepository postRepository,
    ICommentRepository commentRepository,
    IPasswordHasher<User> passwordHasher)
{
    public const string DevelopmentPassword = "password123";

    public async Task ResetAndSeedAsync()
    {
        await DeleteAllAsync(commentRepository.GetManyAsync(), commentRepository.DeleteAsync);
        await DeleteAllAsync(postRepository.GetManyAsync(), postRepository.DeleteAsync);
        await DeleteAllAsync(userRepository.GetManyAsync(), userRepository.DeleteAsync);

        await SeedAsync();
    }

    public async Task SeedAsync(bool includeAdditionalActivity = false)
    {

        User alice = await CreateUserAsync("Alice", UserRole.Admin);
        User bob = await CreateUserAsync("Bob", UserRole.Moderator);
        User charlie = await CreateUserAsync("Charlie", UserRole.User);

        Post firstPost = await postRepository.AddAsync(new Post(
            "First Post",
            "This is the first post.", 
            alice.Id
        ));
        Post secondPost = await postRepository.AddAsync(new Post
        (
            "Second Post",
            "This is the second post.",
            bob.Id
        ));
        Post thirdPost = await postRepository.AddAsync(new Post
        (
            "Third Post",
            "This is the third post.",
            charlie.Id
        ));

        foreach (Post post in new[] { firstPost, secondPost, thirdPost })
        {
            await commentRepository.AddAsync(new Comment ( "This is the first comment.", post.Id, alice.Id ));
            await commentRepository.AddAsync(new Comment ( "This is the second comment.", post.Id, bob.Id ));
            await commentRepository.AddAsync(new Comment ( "This is the third comment.", post.Id, charlie.Id ));
        }

        if (!includeAdditionalActivity) return;

        var users = new List<User>();
        for (int i = 1; i <= 20; i++)
        {
            users.Add(await CreateUserAsync($"SampleUser{i:00}", UserRole.User));
        }

        string[] topics = ["Learning LINQ", "Weekend projects", "Favorite books", "Getting started", "Coffee break"];
        for (int i = 0; i < 40; i++)
        {
            Post post = await postRepository.AddAsync(new Post(
                $"{topics[i % topics.Length]} #{i + 1}",
                $"Sample discussion {i + 1}: sharing ideas and experiences.",
                users[i % users.Count].Id));

            for (int j = 0; j < 2; j++)
            {
                User commenter = users[(i * 3 + j + 1) % users.Count];
                await commentRepository.AddAsync(new Comment(
                    $"Thanks for sharing! Sample reply {i * 2 + j + 1}.", post.Id, commenter.Id));
            }
        }
    }

    private async Task<User> CreateUserAsync(string userName, UserRole role)
    {
        string passwordHash = passwordHasher.HashPassword(null!, DevelopmentPassword);
        var user = new User ( userName, passwordHash, role);

        return await userRepository.AddAsync(user);
    }

    private static async Task DeleteAllAsync<T>(IQueryable<T> entities, Func<int, Task> deleteAsync)
        where T : IEntity
    {
        foreach (T entity in entities.ToArray())
        {
            await deleteAsync(entity.Id);
        }
    }
}
