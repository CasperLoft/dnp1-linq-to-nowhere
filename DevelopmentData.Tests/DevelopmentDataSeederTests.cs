using DevelopmentData;
using Entities;
using Enums;
using FileRepositories;
using Microsoft.AspNetCore.Identity;

namespace DevelopmentData.Tests;

public sealed class DevelopmentDataSeederTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"dnp1-seed-{Guid.NewGuid()}");

    [Fact]
    public async Task ResetAndSeedAsync_CreatesTheCompleteBaselineAndCanBeRepeated()
    {
        var users = new UserFileRepository(_dataDirectory);
        var posts = new PostFileRepository(_dataDirectory);
        var comments = new CommentFileRepository(_dataDirectory);
        var passwordHasher = new PasswordHasher<User>();
        var seeder = new DevelopmentDataSeeder(users, posts, comments, passwordHasher);

        await seeder.ResetAndSeedAsync();

        User[] seededUsers = users.GetManyAsync().OrderBy(user => user.Id).ToArray();
        Assert.Collection(
            seededUsers,
            user => Assert.Equal(("Alice", UserRole.Admin), (user.UserName, user.Role)),
            user => Assert.Equal(("Bob", UserRole.Moderator), (user.UserName, user.Role)),
            user => Assert.Equal(("Charlie", UserRole.User), (user.UserName, user.Role)));
        Assert.All(seededUsers, user => Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DevelopmentDataSeeder.DevelopmentPassword)));

        Post[] seededPosts = posts.GetManyAsync().ToArray();
        Comment[] seededComments = comments.GetManyAsync().ToArray();
        Assert.Equal(3, seededPosts.Length);
        Assert.Equal(9, seededComments.Length);
        Assert.All(seededPosts, post => Assert.Contains(seededUsers, user => user.Id == post.UserId));
        Assert.All(seededComments, comment =>
        {
            Assert.Contains(seededUsers, user => user.Id == comment.UserId);
            Assert.Contains(seededPosts, post => post.Id == comment.PostId);
        });

        await seeder.ResetAndSeedAsync();

        Assert.Equal(3, users.GetManyAsync().Count());
        Assert.Equal(3, posts.GetManyAsync().Count());
        Assert.Equal(9, comments.GetManyAsync().Count());
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, recursive: true);
        }
    }
}
