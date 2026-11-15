using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RepositoryContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WebAPI.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public TestUserRepository Users { get; } = new();
    public TestPostRepository Posts { get; } = new();
    public TestCommentRepository Comments { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "TestAdmin"; options.DefaultChallengeScheme = "TestAdmin"; options.DefaultForbidScheme = "TestAdmin"; })
                .AddScheme<AuthenticationSchemeOptions, TestAdminHandler>("TestAdmin", _ => { });
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IPostRepository>();
            services.RemoveAll<ICommentRepository>();
            services.AddSingleton<IUserRepository>(Users);
            services.AddSingleton<IPostRepository>(Posts);
            services.AddSingleton<ICommentRepository>(Comments);
        });
    }
}

public class TestAdminHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestAdmin"), new Claim(ClaimTypes.Role, "Admin")], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}

public class TestRepository<T> where T : IEntity
{
    public List<T> Items { get; } = [];

    public void Reset(params T[] items)
    {
        Items.Clear();
        Items.AddRange(items);
    }

    public Task<T> AddAsync(T entity)
    {
        entity.Id = Items.Count == 0 ? 1 : Items.Max(item => item.Id) + 1;
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        int index = Items.FindIndex(item => item.Id == entity.Id);
        if (index < 0) throw new InvalidOperationException();
        Items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        T? entity = Items.SingleOrDefault(item => item.Id == id);
        if (entity is null) throw new InvalidOperationException();
        Items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<T> GetByIdAsync(int id) => Task.FromResult(Items.SingleOrDefault(item => item.Id == id)
        ?? throw new InvalidOperationException());

    public IQueryable<T> GetManyAsync() => Items.AsQueryable();
}

public sealed class TestUserRepository : TestRepository<User>, IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username) =>
        Task.FromResult(Items.FirstOrDefault(user =>
            string.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase)));
}
public sealed class TestPostRepository : TestRepository<Post>, IPostRepository;
public sealed class TestCommentRepository : TestRepository<Comment>, ICommentRepository;
