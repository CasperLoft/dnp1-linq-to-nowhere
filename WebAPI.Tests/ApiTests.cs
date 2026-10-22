using System.Net;
using System.Net.Http.Json;
using DTOs;
using DTOs.Comments.Requests;
using Entities;
using Enums;

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

        HttpResponseMessage response = await _client.PostAsJsonAsync("/users", new CreateUserRequest("ada", "secret", UserRole.User));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/users/1", response.Headers.Location?.AbsolutePath);
        Assert.Equal(new UserResponse(1, "ada", UserRole.User), await response.Content.ReadFromJsonAsync<UserResponse>());
    }

    [Fact]
    public async Task GetUsers_FiltersByUserName()
    {
        _factory.Users.Reset(
            new User { Id = 1, UserName = "Ada", PasswordHash = "secret" },
            new User { Id = 2, UserName = "Bob", PasswordHash = "secret" });

        HttpResponseMessage response = await _client.GetAsync("/users?userNameContains=ad");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserResponse[] users = await response.Content.ReadFromJsonAsync<UserResponse[]>()
            ?? throw new InvalidOperationException("The users response was empty.");
        Assert.Equal([new UserResponse(1, "Ada", UserRole.User)], users);
    }

    [Fact]
    public async Task GetUser_ReturnsUserDto()
    {
        _factory.Users.Reset(new User { Id = 2, UserName = "Bob", PasswordHash = "secret" });

        HttpResponseMessage response = await _client.GetAsync("/users/2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new UserResponse(2, "Bob", UserRole.User), await response.Content.ReadFromJsonAsync<UserResponse>());
    }

    [Fact]
    public async Task GetPosts_PassesAllFiltersToService()
    {
        _factory.Users.Reset(new User { Id = 2, UserName = "ada", PasswordHash = "secret" });
        _factory.Posts.Reset(new Post { Id = 3, UserId = 2, Title = "LINQ", Body = "Body" });

        HttpResponseMessage response = await _client.GetAsync("/posts?userId=2&titleContains=linq&userNameContains=ada");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PostResponse post = Assert.Single(await response.Content.ReadFromJsonAsync<PostResponse[]>()
            ?? throw new InvalidOperationException("The posts response was empty."));
        Assert.Equal("ada", post.UserName);
    }

    [Fact]
    public async Task CreatePost_ReturnsCreatedDtoWithAuthorUserName()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", PasswordHash = "secret" });
        _factory.Posts.Reset();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/posts",
            new CreatePostRequest("A new post", "Post body", 1));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/posts/1", response.Headers.Location?.AbsolutePath);

        PostResponse createdPost = await response.Content.ReadFromJsonAsync<PostResponse>()
            ?? throw new InvalidOperationException("The created post response was empty.");
        Assert.Equal(1, createdPost.Id);
        Assert.Equal("A new post", createdPost.Title);
        Assert.Equal("Post body", createdPost.Body);
        Assert.Equal("alice", createdPost.UserName);
    }

    [Fact]
    public async Task GetComments_ReturnsUserName()
    {
        _factory.Users.Reset(new User { Id = 2, UserName = "ada", PasswordHash = "secret" });
        _factory.Comments.Reset(new Comment { Id = 4, UserId = 2, PostId = 3, Body = "Nice post" });

        HttpResponseMessage response = await _client.GetAsync("/comments?postId=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        CommentResponse comment = Assert.Single(await response.Content.ReadFromJsonAsync<CommentResponse[]>()
            ?? throw new InvalidOperationException("The comments response was empty."));
        Assert.Equal("ada", comment.UserName);
    }

    [Fact]
    public async Task CreateComment_ReturnsCreatedDtoWithAuthorUserName()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", PasswordHash = "secret" });
        _factory.Posts.Reset(new Post { Id = 3, UserId = 1, Title = "Post", Body = "Body" });
        _factory.Comments.Reset();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/comments",
            new CreateCommentRequest("A new comment", 1, 3));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/comments/1", response.Headers.Location?.AbsolutePath);

        CommentResponse createdComment = await response.Content.ReadFromJsonAsync<CommentResponse>()
            ?? throw new InvalidOperationException("The created comment response was empty.");
        Assert.Equal(1, createdComment.Id);
        Assert.Equal("A new comment", createdComment.Body);
        Assert.Equal(3, createdComment.PostId);
        Assert.Equal("alice", createdComment.UserName);
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
        _factory.Users.Reset(new User { Id = 1, UserName = "ada", PasswordHash = "secret" });

        HttpResponseMessage response = await _client.PostAsJsonAsync("/users", new CreateUserRequest("ada", "secret", UserRole.User));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
