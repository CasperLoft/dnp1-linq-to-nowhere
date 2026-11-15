using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using RepositoryContracts.Exceptions;

namespace EfcRepositories;

public class EfcPostRepository : IPostRepository
{
    private readonly AppContext _ctx;
    
    public EfcPostRepository(AppContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Post> AddAsync(Post post)
    {
        await _ctx.Posts.AddAsync(post);
        await _ctx.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        if (!(await _ctx.Posts.AnyAsync(p => p.Id == post.Id)))
        {
            throw new NotFoundException($"Post with {post.Id} not found");
        }

        _ctx.Posts.Update(post);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int postId)
    {
        Post? existing = await _ctx.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (existing == null)
        {
            throw new NotFoundException($"Post with {postId} not found");
        }

        _ctx.Posts.Remove(existing);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Post> GetByIdAsync(int postId)
    {
        Post? existing = await _ctx.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (existing == null)
        {
            throw new NotFoundException($"Post with {postId} not found");
        }

        return existing;
    }

    public IQueryable<Post> GetManyAsync()
    {
        return _ctx.Posts.AsQueryable();
    }
}