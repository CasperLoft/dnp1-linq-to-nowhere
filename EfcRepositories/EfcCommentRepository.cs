using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using RepositoryContracts.Exceptions;

namespace EfcRepositories;

public class EfcCommentRepository : ICommentRepository
{
    private readonly AppContext _ctx;
    
    public EfcCommentRepository(AppContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        await _ctx.Comments.AddAsync(comment);
        await _ctx.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        if (!(await _ctx.Comments.AnyAsync(c => c.Id == comment.Id)))
        {
            throw new NotFoundException($"Comment with {comment.Id} not found");
        }
        
        _ctx.Comments.Update(comment);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int commentId)
    {
        Comment? existing = await _ctx.Comments.SingleOrDefaultAsync(c => c.Id == commentId);

        if (existing == null)
        {
            throw new NotFoundException($"Comment with {commentId} not found");
        }

        _ctx.Comments.Remove(existing);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Comment> GetByIdAsync(int commentId)
    {
        Comment? existing = await _ctx.Comments.SingleOrDefaultAsync(c => c.Id == commentId);
        if (existing == null)
        {
            throw new NotFoundException($"Post with {commentId} not found");
        }

        return existing;
    }
    public IQueryable<Comment> GetManyAsync()
    {
        return _ctx.Comments.AsQueryable();
    }
}