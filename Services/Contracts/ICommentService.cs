using DTOs;
using DTOs.Comments.Requests;

namespace Services;

public interface ICommentService
{
    Task<CommentResponse> CreateAsync(CreateCommentRequest request);
    Task<CommentResponse> GetByIdAsync(int id);
    Task<IReadOnlyCollection<CommentResponse>> GetManyAsync(
        int? userId = null,
        int? postId = null,
        string? userNameContains = null);
    Task<CommentResponse> UpdateAsync(int id, UpdateCommentRequest request);
    Task DeleteAsync(int id);
}
