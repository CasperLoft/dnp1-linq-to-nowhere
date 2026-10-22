using DTOs;

namespace WebApp.Services;

public interface ICommentService
{
    public Task<CommentDTO> AddCommentAsync(CreateCommentDTO request);
    public Task UpdateCommentAsync(UpdateCommentDTO request);
    public Task<IReadOnlyCollection<CommentDTO>> GetManyAsync(int? postId = null);
}