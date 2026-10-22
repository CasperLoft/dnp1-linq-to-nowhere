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
        PostDTO post = Assert.Single(await response.Content.ReadFromJsonAsync<PostDTO[]>()
            ?? throw new InvalidOperationException("The posts response was empty."));
        Assert.Equal("ada", post.UserName);
    }

    [Fact]
    public async Task CreatePost_ReturnsCreatedDtoWithAuthorUserName()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", Password = "secret" });
        _factory.Posts.Reset();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/posts",
            new CreatePostDTO("A new post", "Post body", 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/posts/1", response.Headers.Location?.AbsolutePath);

        PostDTO createdPost = await response.Content.ReadFromJsonAsync<PostDTO>()
            ?? throw new InvalidOperationException("The created post response was empty.");
        Assert.Equal(1, createdPost.Id);
        Assert.Equal("A new post", createdPost.Title);
        Assert.Equal("Post body", createdPost.Body);
        Assert.Equal("alice", createdPost.UserName);
    }

    [Fact]
    public async Task GetComments_ReturnsUserName()
    {
        _factory.Users.Reset(new User { Id = 2, UserName = "ada", Password = "secret" });
        _factory.Comments.Reset(new Comment { Id = 4, UserId = 2, PostId = 3, Body = "Nice post" });

        HttpResponseMessage response = await _client.GetAsync("/comments?postId=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        CommentDTO comment = Assert.Single(await response.Content.ReadFromJsonAsync<CommentDTO[]>()
            ?? throw new InvalidOperationException("The comments response was empty."));
        Assert.Equal("ada", comment.UserName);
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
