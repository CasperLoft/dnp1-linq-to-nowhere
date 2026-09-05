namespace RepositoryContracts;

using Entities;

public interface ICommentRepository
{
    Task<Comment> AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(int commentId);
    Task<Comment> GetByIdAsync(int commentId);
    IQueryable<Comment> GetManyAsync();
}