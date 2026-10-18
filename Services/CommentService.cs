using DTOs;
using Entities;
using RepositoryContracts;

namespace Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IPostRepository postRepository)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _postRepository = postRepository;
    }

    public async Task<CommentDTO> CreateAsync(CreateCommentDTO request)
    {
        await EnsureReferencesExistAsync(request.UserId, request.PostId);

        Comment created = await _commentRepository.AddAsync(new Comment
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId
        });

        return ToDto(created);
    }

    public async Task<CommentDTO> GetByIdAsync(int id)
    {
        Comment comment = await _commentRepository.GetByIdAsync(id);
        return ToDto(comment);
    }

    public Task<IReadOnlyCollection<CommentDTO>> GetManyAsync(int? userId = null, int? postId = null)
    {
        IQueryable<Comment> comments = _commentRepository.GetManyAsync();

        if (userId.HasValue)
        {
            comments = comments.Where(comment => comment.UserId == userId.Value);
        }

        if (postId.HasValue)
        {
            comments = comments.Where(comment => comment.PostId == postId.Value);
        }

        IReadOnlyCollection<CommentDTO> result = comments.Select(ToDto).ToArray();
        return Task.FromResult(result);
    }

    public async Task<CommentDTO> UpdateAsync(int id, UpdateCommentDTO request)
    {
        Comment comment = await _commentRepository.GetByIdAsync(id);
        await EnsureReferencesExistAsync(comment.UserId, comment.PostId);

        comment.Body = request.Body;
        await _commentRepository.UpdateAsync(comment);

        return ToDto(comment);
    }

    public Task DeleteAsync(int id) => _commentRepository.DeleteAsync(id);

    private async Task EnsureReferencesExistAsync(int userId, int postId)
    {
        await _userRepository.GetByIdAsync(userId);
        await _postRepository.GetByIdAsync(postId);
    }

    private static CommentDTO ToDto(Comment comment) =>
        new(comment.Id, comment.Body, comment.UserId, comment.PostId);
}
