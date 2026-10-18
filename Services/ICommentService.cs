using DTOs;

namespace Services;

public interface ICommentService
{
    Task<CommentDTO> CreateAsync(CreateCommentDTO request);
    Task<CommentDTO> GetByIdAsync(int id);
    Task<IReadOnlyCollection<CommentDTO>> GetManyAsync(int? userId = null, int? postId = null);
    Task<CommentDTO> UpdateAsync(int id, UpdateCommentDTO request);
    Task DeleteAsync(int id);
}
