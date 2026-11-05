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
