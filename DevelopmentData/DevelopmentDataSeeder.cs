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

        Post firstPost = await postRepository.AddAsync(new Post
        {
            Title = "First Post",
            Body = "This is the first post.",
            UserId = alice.Id
        });
        Post secondPost = await postRepository.AddAsync(new Post
        {
            Title = "Second Post",
            Body = "This is the second post.",
            UserId = bob.Id
        });
        Post thirdPost = await postRepository.AddAsync(new Post
        {
            Title = "Third Post",
            Body = "This is the third post.",
            UserId = charlie.Id
        });

        foreach (Post post in new[] { firstPost, secondPost, thirdPost })
        {
            await commentRepository.AddAsync(new Comment { Body = "This is the first comment.", PostId = post.Id, UserId = alice.Id });
            await commentRepository.AddAsync(new Comment { Body = "This is the second comment.", PostId = post.Id, UserId = bob.Id });
            await commentRepository.AddAsync(new Comment { Body = "This is the third comment.", PostId = post.Id, UserId = charlie.Id });
        }
    }

    private async Task<User> CreateUserAsync(string userName, UserRole role)
    {
        var user = new User { UserName = userName, Role = role };
        user.PasswordHash = passwordHasher.HashPassword(user, DevelopmentPassword);

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
