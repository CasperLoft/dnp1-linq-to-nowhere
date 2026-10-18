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
            new Comment { Id = 1, UserId = 2, PostId = 3, Body = "Keep" },
            new Comment { Id = 2, UserId = 2, PostId = 4, Body = "Wrong post" },
            new Comment { Id = 3, UserId = 5, PostId = 3, Body = "Wrong user" }
        }.AsQueryable());

        var service = new CommentService(comments.Object, Mock.Of<IUserRepository>(), Mock.Of<IPostRepository>());

        IReadOnlyCollection<CommentDTO> result = await service.GetManyAsync(userId: 2, postId: 3);

        Assert.Collection(result, comment => Assert.Equal(new CommentDTO(1, "Keep", 2, 3), comment));
    }
}
