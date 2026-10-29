using DTOs;
using DTOs.Comments.Requests;

namespace WebApp.Services;

public interface ICommentService
{
    public Task<CommentResponse> AddCommentAsync(CreateCommentRequest request);
    public Task<CommentResponse> UpdateCommentAsync(int id, UpdateCommentRequest request);
    public Task DeleteCommentAsync(int id);
    public Task<IReadOnlyCollection<CommentResponse>> GetManyAsync(int? postId = null, int? userId = null);
}
