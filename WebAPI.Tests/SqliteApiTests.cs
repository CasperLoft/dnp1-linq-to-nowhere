using System.Net;
using System.Net.Http.Json;
using DTOs;
using DTOs.Auth.Requests;
using DTOs.Comments.Requests;
using Entities;
using Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RepositoryContracts;
using EfcRepositories;
using DatabaseContext = EfcRepositories.AppContext;

namespace WebAPI.Tests;

public class SqliteApiTests
{
    private sealed class SqliteFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"linq-api-{Guid.NewGuid():N}.db");
        private string ConnectionString => new SqliteConnectionStringBuilder { DataSource = _databasePath, Pooling = false }.ToString();

        public async Task MigrateAsync()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(ConnectionString).Options;
            await using var context = new DatabaseContext(options);
            await context.Database.MigrateAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DatabaseContext>();
                services.RemoveAll<DbContextOptions<DatabaseContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<DatabaseContext>>();
                services.AddDbContext<DatabaseContext>(options => options.UseSqlite(ConnectionString));
            });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_databasePath)) File.Delete(_databasePath);
        }
    }

    [Fact]
    public async Task EfRepositoriesPersistAndSupportApiOperations()
    {
        using var factory = new SqliteFactory();
        await factory.MigrateAsync();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            Assert.IsType<EfcUserRepository>(scope.ServiceProvider.GetRequiredService<IUserRepository>());
            Assert.IsType<EfcPostRepository>(scope.ServiceProvider.GetRequiredService<IPostRepository>());
            Assert.IsType<EfcCommentRepository>(scope.ServiceProvider.GetRequiredService<ICommentRepository>());
        }

        var createUser = await client.PostAsJsonAsync("/users", new CreateUserRequest("Ada", "secret", UserRole.User));
        Assert.Equal(HttpStatusCode.Created, createUser.StatusCode);
        var user = (await createUser.Content.ReadFromJsonAsync<UserResponse>())!;
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/users", new CreateUserRequest("ada", "secret", UserRole.User))).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<UserResponse[]>("/users?userNameContains=AD"))!);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/auth/login", new LoginRequest("missing", "secret"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/auth/login", new LoginRequest("ada", "secret"))).StatusCode);

        var createPost = await client.PostAsJsonAsync("/posts", new CreatePostRequest("LINQ Test", "Body", user.Id));
        Assert.Equal(HttpStatusCode.Created, createPost.StatusCode);
        var post = (await createPost.Content.ReadFromJsonAsync<PostResponse>())!;
        var createComment = await client.PostAsJsonAsync("/comments", new CreateCommentRequest("Hello", user.Id, post.Id));
        Assert.Equal(HttpStatusCode.Created, createComment.StatusCode);
        var comment = (await createComment.Content.ReadFromJsonAsync<CommentResponse>())!;
        Assert.Equal("Ada", (await client.GetFromJsonAsync<PostResponse>($"/posts/{post.Id}"))!.UserName);
        Assert.Equal("Ada", (await client.GetFromJsonAsync<CommentResponse>($"/comments/{comment.Id}"))!.UserName);
        Assert.Single((await client.GetFromJsonAsync<PostResponse[]>($"/posts?userId={user.Id}&titleContains=linq&userNameContains=AD"))!);
        Assert.Single((await client.GetFromJsonAsync<CommentResponse[]>($"/comments?userId={user.Id}&postId={post.Id}&userNameContains=AD"))!);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/users/{user.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/posts/{post.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/users/{user.Id}", new UpdateUserRequest("Grace", UserRole.Admin))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/posts/{post.Id}", new UpdatePostRequest("Updated", "New body"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/comments/{comment.Id}", new UpdateCommentRequest("Updated comment"))).StatusCode);
        Assert.Equal("Grace", (await client.GetFromJsonAsync<UserResponse>($"/users/{user.Id}"))!.UserName);
        Assert.Equal("Updated", (await client.GetFromJsonAsync<PostResponse>($"/posts/{post.Id}"))!.Title);
        Assert.Equal("Updated comment", (await client.GetFromJsonAsync<CommentResponse>($"/comments/{comment.Id}"))!.Body);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/comments/{comment.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/posts/{post.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/users/{user.Id}")).StatusCode);
        foreach (string resource in new[] { $"/users/{user.Id}", $"/posts/{post.Id}", $"/comments/{comment.Id}" })
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(resource)).StatusCode);
    }

    [Fact]
    public async Task SqliteUniqueConstraintIsMappedToConflict()
    {
        using var factory = new SqliteFactory();
        await factory.MigrateAsync();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/users", new CreateUserRequest("Ada", "secret", UserRole.User))).StatusCode);
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(new User("ada", "hash")));
        var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        httpContext.Response.Body = new MemoryStream();
        Assert.True(await new ApiExceptionHandler().TryHandleAsync(httpContext, exception, CancellationToken.None));
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }
}
