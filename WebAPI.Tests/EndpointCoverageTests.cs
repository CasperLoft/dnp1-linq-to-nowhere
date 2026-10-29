using System.Net;
using System.Net.Http.Json;
using System.Text;
using DTOs;
using DTOs.Auth.Requests;
using DTOs.Comments.Requests;
using Entities;
using Enums;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Tests;

public class EndpointCoverageTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public EndpointCoverageTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.Users.Reset();
        _factory.Posts.Reset();
        _factory.Comments.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUserWithoutPassword()
    {
        var created = await _client.PostAsJsonAsync("/users", new CreateUserRequest("alice", "secret", UserRole.Moderator));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest("alice", "secret"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new UserResponse(1, "alice", UserRole.Moderator), await response.Content.ReadFromJsonAsync<UserResponse>());
        Assert.DoesNotContain("password", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("unknown", "secret")]
    [InlineData("alice", "wrong")]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized(string username, string password)
    {
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/users", new CreateUserRequest("alice", "secret", UserRole.User))).StatusCode);
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(username, password));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPostAndComment_ReturnTheirDtos()
    {
        SeedGraph();
        var post = await _client.GetAsync("/posts/3");
        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        Assert.Equal(new PostResponse(3, "Title", "Body", 1, "alice"), await post.Content.ReadFromJsonAsync<PostResponse>());
        var comment = await _client.GetAsync("/comments/4");
        Assert.Equal(HttpStatusCode.OK, comment.StatusCode);
        Assert.Equal(new CommentResponse(4, "Comment", 1, 3, "alice"), await comment.Content.ReadFromJsonAsync<CommentResponse>());
    }

    [Fact]
    public async Task UpdateUser_ReturnsAndPersistsUpdatedDto()
    {
        SeedGraph();
        var expected = new UserResponse(1, "alicia", UserRole.Admin);
        var response = await _client.PutAsJsonAsync("/users/1", new UpdateUserRequest("alicia", UserRole.Admin));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, await response.Content.ReadFromJsonAsync<UserResponse>());
        Assert.Equal(expected, await _client.GetFromJsonAsync<UserResponse>("/users/1"));
    }

    [Fact]
    public async Task UpdatePost_ReturnsAndPersistsUpdatedDto()
    {
        SeedGraph();
        var expected = new PostResponse(3, "Updated", "New body", 1, "alice");
        var response = await _client.PutAsJsonAsync("/posts/3", new UpdatePostRequest("Updated", "New body"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, await response.Content.ReadFromJsonAsync<PostResponse>());
        Assert.Equal(expected, await _client.GetFromJsonAsync<PostResponse>("/posts/3"));
    }

    [Fact]
    public async Task UpdateComment_ReturnsAndPersistsUpdatedDto()
    {
        SeedGraph();
        var expected = new CommentResponse(4, "Updated", 1, 3, "alice");
        var response = await _client.PutAsJsonAsync("/comments/4", new UpdateCommentRequest("Updated"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, await response.Content.ReadFromJsonAsync<CommentResponse>());
        Assert.Equal(expected, await _client.GetFromJsonAsync<CommentResponse>("/comments/4"));
    }

    [Fact]
    public async Task DeleteResources_RemovesEachResource()
    {
        SeedGraph();
        foreach (string path in new[] { "/comments/4", "/posts/3", "/users/1" })
        {
            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync(path)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(path)).StatusCode);
        }
    }

    [Theory]
    [InlineData("users")]
    [InlineData("posts")]
    [InlineData("comments")]
    public async Task MissingResource_GetUpdateAndDelete_ReturnNotFound(string resource)
    {
        string path = $"/{resource}/99";
        var get = await _client.GetAsync(path);
        await AssertProblem(get, HttpStatusCode.NotFound, "Resource not found", path);
        HttpResponseMessage update = resource switch
        {
            "users" => await _client.PutAsJsonAsync(path, new UpdateUserRequest("alice", UserRole.User)),
            "posts" => await _client.PutAsJsonAsync(path, new UpdatePostRequest("Title", "Body")),
            _ => await _client.PutAsJsonAsync(path, new UpdateCommentRequest("Body"))
        };
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(path)).StatusCode);
    }

    [Fact]
    public async Task CreateWithMissingReferences_ReturnsNotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsJsonAsync("/posts", new CreatePostRequest("Title", "Body", 99))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsJsonAsync("/comments", new CreateCommentRequest("Body", 99, 3))).StatusCode);
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", PasswordHash = "unused" });
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsJsonAsync("/comments", new CreateCommentRequest("Body", 1, 99))).StatusCode);
    }

    [Fact]
    public async Task UpdateUser_DuplicateName_ReturnsConflictWithoutChangingUser()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", PasswordHash = "unused" }, new User { Id = 2, UserName = "bob", PasswordHash = "unused" });
        var response = await _client.PutAsJsonAsync("/users/2", new UpdateUserRequest("ALICE", UserRole.User));
        await AssertProblem(response, HttpStatusCode.Conflict, "Request conflicts with existing data", "/users/2");
        Assert.Equal(new UserResponse(2, "bob", UserRole.User), await _client.GetFromJsonAsync<UserResponse>("/users/2"));
    }

    [Fact]
    public async Task DeleteWithDependents_ReturnsConflictAndPreservesResource()
    {
        SeedGraph();
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync("/users/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync("/posts/3")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/users/1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/posts/3")).StatusCode);
        _factory.Posts.Reset();
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync("/users/1")).StatusCode);
    }

    [Fact]
    public async Task InvalidBodyAndQuery_ReturnBadRequest()
    {
        var malformed = await _client.PostAsync("/users", new StringContent("{", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync("/posts?userId=not-a-number")).StatusCode);
    }

    [Theory]
    [InlineData("/posts?userId=1", 3)]
    [InlineData("/posts?titleContains=linq", 3)]
    [InlineData("/posts?userNameContains=ali", 3)]
    public async Task PostFilters_ExcludeNonmatchingRecords(string path, int expectedId)
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice" }, new User { Id = 2, UserName = "bob" });
        _factory.Posts.Reset(new Post { Id = 3, UserId = 1, Title = "LINQ", Body = "Body" }, new Post { Id = 4, UserId = 2, Title = "Other", Body = "Body" });
        var result = await _client.GetFromJsonAsync<PostResponse[]>(path);
        Assert.Equal(expectedId, Assert.Single(result!).Id);
    }

    [Theory]
    [InlineData("/comments?userId=1")]
    [InlineData("/comments?postId=3")]
    [InlineData("/comments?userNameContains=ali")]
    public async Task CommentFilters_ExcludeNonmatchingRecords(string path)
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice" }, new User { Id = 2, UserName = "bob" });
        _factory.Comments.Reset(new Comment { Id = 4, UserId = 1, PostId = 3, Body = "One" }, new Comment { Id = 5, UserId = 2, PostId = 6, Body = "Two" });
        var result = await _client.GetFromJsonAsync<CommentResponse[]>(path);
        Assert.Equal(4, Assert.Single(result!).Id);
    }

    private void SeedGraph()
    {
        _factory.Users.Reset(new User { Id = 1, UserName = "alice", PasswordHash = "unused" });
        _factory.Posts.Reset(new Post { Id = 3, UserId = 1, Title = "Title", Body = "Body" });
        _factory.Comments.Reset(new Comment { Id = 4, UserId = 1, PostId = 3, Body = "Comment" });
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string title, string path)
    {
        Assert.Equal(status, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem.Status);
        Assert.Equal(title, problem.Title);
        Assert.Equal(path, problem.Instance);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }
}
