using System.Net;
using System.Net.Http.Json;
using DTOs;
using Entities;

namespace WebAPI.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedDtoAndLocation()
    {
        _factory.Users.Reset();

        HttpResponseMessage response = await _client.PostAsJsonAsync("/users", new CreateUserDTO("ada", "secret"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/users/1", response.Headers.Location?.AbsolutePath);
        Assert.Equal(new UserDTO(1, "ada"), await response.Content.ReadFromJsonAsync<UserDTO>());
    }

    [Fact]
    public async Task GetPosts_PassesAllFiltersToService()
    {
        _factory.Users.Reset(new User { Id = 2, UserName = "ada", Password = "secret" });
        _factory.Posts.Reset(new Post { Id = 3, UserId = 2, Title = "LINQ", Body = "Body" });

        HttpResponseMessage response = await _client.GetAsync("/posts?userId=2&titleContains=linq&userNameContains=ada");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("LINQ", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetComment_MapsMissingResourceToNotFoundProblem()
    {
        _factory.Comments.Reset();

        HttpResponseMessage response = await _client.GetAsync("/comments/9");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_MapsDuplicateNameToConflictProblem()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "ada", Password = "secret" });

        HttpResponseMessage response = await _client.PostAsJsonAsync("/users", new CreateUserDTO("ada", "secret"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
