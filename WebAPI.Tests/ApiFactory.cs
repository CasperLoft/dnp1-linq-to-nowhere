using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RepositoryContracts;

namespace WebAPI.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public TestUserRepository Users { get; } = new();
    public TestPostRepository Posts { get; } = new();
    public TestCommentRepository Comments { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IPostRepository>();
            services.RemoveAll<ICommentRepository>();
            services.AddSingleton<IUserRepository>(Users);
            services.AddSingleton<IPostRepository>(Posts);
            services.AddSingleton<ICommentRepository>(Comments);
        });
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

public sealed class TestUserRepository : TestRepository<User>, IUserRepository;
public sealed class TestPostRepository : TestRepository<Post>, IPostRepository;
public sealed class TestCommentRepository : TestRepository<Comment>, ICommentRepository;
