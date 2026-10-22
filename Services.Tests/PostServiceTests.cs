using DTOs;
using Entities;
using Moq;
using RepositoryContracts;
using Services;

namespace Services.Tests;

public class PostServiceTests
{
    [Fact]
    public async Task CreateAsync_ThrowsWhenAuthorDoesNotExist()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetByIdAsync(99))
            .ThrowsAsync(new InvalidOperationException("Entity with Id 99 not found."));

        var posts = new Mock<IPostRepository>();
        var service = new PostService(posts.Object, users.Object, Mock.Of<ICommentRepository>());

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.CreateAsync(new CreatePostRequest("Title", "Body", 99)));
        posts.Verify(repository => repository.AddAsync(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task GetManyAsync_AppliesAuthorAndCaseInsensitiveTitleFilters()
    {
        var posts = new Mock<IPostRepository>();
        posts.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new Post { Id = 1, UserId = 2, Title = "Learning LINQ", Body = "Body" },
            new Post { Id = 2, UserId = 2, Title = "Other", Body = "Body" },
            new Post { Id = 3, UserId = 3, Title = "LINQ Tips", Body = "Body" }
        }.AsQueryable());

        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new User { Id = 2, UserName = "ada", PasswordHash = "secret" },
            new User { Id = 3, UserName = "grace", PasswordHash = "secret" }
        }.AsQueryable());

        var service = new PostService(posts.Object, users.Object, Mock.Of<ICommentRepository>());

        IReadOnlyCollection<PostResponse> result = await service.GetManyAsync(2, "linq");

        Assert.Collection(result, post => Assert.Equal(new PostResponse(1, "Learning LINQ", "Body", 2, "ada"), post));
    }

    [Fact]
    public async Task DeleteAsync_RejectsPostWithComments()
    {
        var posts = new Mock<IPostRepository>();
        posts.Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(new Post { Id = 1, UserId = 1, Title = "Title", Body = "Body" });

        var comments = new Mock<ICommentRepository>();
        comments.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new Comment { Id = 3, PostId = 1, UserId = 1, Body = "Comment" }
        }.AsQueryable());

        var service = new PostService(posts.Object, Mock.Of<IUserRepository>(), comments.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));
        posts.Verify(repository => repository.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
