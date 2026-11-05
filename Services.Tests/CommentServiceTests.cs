using DTOs;
using Entities;
using Moq;
using RepositoryContracts;
using Services;

namespace Services.Tests;

public class CommentServiceTests
{
    [Fact]
    public async Task GetManyAsync_AppliesUserAndPostFilters()
    {
        var comments = new Mock<ICommentRepository>();
        comments.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new Comment("Keep", 3, 2) { Id = 1 },
            new Comment("Wrong post", 4, 2) { Id = 2 },
            new Comment("Wrong user", 3, 5) { Id = 3 }
        }.AsQueryable());

        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new User("ada", "secret") { Id = 2 },
            new User("grace", "secret") { Id = 5 }
        }.AsQueryable());

        var service = new CommentService(comments.Object, users.Object, Mock.Of<IPostRepository>());

        IReadOnlyCollection<CommentResponse> result = await service.GetManyAsync(userId: 2, postId: 3);

        Assert.Collection(result, comment => Assert.Equal(new CommentResponse(1, "Keep", 2, 3, "ada"), comment));
    }
}