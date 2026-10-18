using DTOs;
using Entities;
using Moq;
using RepositoryContracts;
using Services;

namespace Services.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_MapsCreatedUserToDto()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetManyAsync()).Returns(Enumerable.Empty<User>().AsQueryable());
        users.Setup(repository => repository.AddAsync(It.IsAny<User>())).ReturnsAsync((User user) =>
        {
            user.Id = 4;
            return user;
        });

        var service = new UserService(users.Object, Mock.Of<IPostRepository>(), Mock.Of<ICommentRepository>());

        UserDTO result = await service.CreateAsync(new CreateUserDTO("ada", "secret"));

        Assert.Equal(new UserDTO(4, "ada"), result);
        users.Verify(repository => repository.AddAsync(It.Is<User>(user =>
            user.UserName == "ada" && user.Password == "secret")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateUserNameIgnoringCase()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new User { Id = 1, UserName = "Ada", Password = "secret" }
        }.AsQueryable());

        var service = new UserService(users.Object, Mock.Of<IPostRepository>(), Mock.Of<ICommentRepository>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateUserDTO("ada", "another-secret")));
        users.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RejectsUserWithPosts()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, UserName = "ada", Password = "secret" });

        var posts = new Mock<IPostRepository>();
        posts.Setup(repository => repository.GetManyAsync()).Returns(new[]
        {
            new Post { Id = 8, UserId = 1, Title = "Title", Body = "Body" }
        }.AsQueryable());

        var service = new UserService(users.Object, posts.Object, Mock.Of<ICommentRepository>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1));
        users.Verify(repository => repository.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
