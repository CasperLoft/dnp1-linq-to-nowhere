using DTOs;
using DTOs.Comments.Requests;

namespace WebApp.Services;

public interface ICommentService
{
    public Task<CommentResponse> AddCommentAsync(CreateCommentRequest request);
    public Task UpdateCommentAsync(UpdateCommentRequest request);
    public Task<IReadOnlyCollection<CommentResponse>> GetManyAsync(int? postId = null, int? userId = null);
}
