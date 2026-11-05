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
            new Post("Learning LINQ", "Body", 2) { Id = 1 },
            new Post("Other", "Body", 2) { Id = 2 },
            new Post("LINQ Tips", "Body", 3) { Id = 3 }
        }.AsQueryable());

        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new User("ada", "secret") { Id = 2 },
            new User("grace", "secret") { Id = 3 }
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
            .ReturnsAsync(new Post("Title", "Body", 1) { Id = 1 });

        var comments = new Mock<ICommentRepository>();
        comments.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new Comment("Comment", 1, 1) { Id = 3 }
        }.AsQueryable());

        var service = new PostService(posts.Object, Mock.Of<IUserRepository>(), comments.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));
        posts.Verify(repository => repository.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
