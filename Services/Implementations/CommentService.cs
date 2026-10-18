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
        Comment comment = await GetCommentAsync(id);
        return ToDto(comment);
    }

    public Task<IReadOnlyCollection<CommentDTO>> GetManyAsync(
        int? userId = null,
        int? postId = null,
        string? userNameContains = null)
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

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            HashSet<int> matchingUserIds = _userRepository.GetManyAsync()
                .Where(user => user.UserName.Contains(userNameContains, StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Id)
                .ToHashSet();

            comments = comments.Where(comment => matchingUserIds.Contains(comment.UserId));
        }

        IReadOnlyCollection<CommentDTO> result = comments.Select(ToDto).ToArray();
        return Task.FromResult(result);
    }

    public async Task<CommentDTO> UpdateAsync(int id, UpdateCommentDTO request)
    {
        Comment comment = await GetCommentAsync(id);
        await EnsureReferencesExistAsync(comment.UserId, comment.PostId);

        comment.Body = request.Body;
        await _commentRepository.UpdateAsync(comment);

        return ToDto(comment);
    }

    public async Task DeleteAsync(int id)
    {
        await GetCommentAsync(id);
        await _commentRepository.DeleteAsync(id);
    }

    private async Task EnsureReferencesExistAsync(int userId, int postId)
    {
        try
        {
            await _userRepository.GetByIdAsync(userId);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"User with Id {userId} was not found.", exception);
        }

        try
        {
            await _postRepository.GetByIdAsync(postId);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"Post with Id {postId} was not found.", exception);
        }
    }

    private async Task<Comment> GetCommentAsync(int id)
    {
        try
        {
            return await _commentRepository.GetByIdAsync(id);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"Comment with Id {id} was not found.", exception);
        }
    }

    private static CommentDTO ToDto(Comment comment) =>
        new(comment.Id, comment.Body, comment.UserId, comment.PostId);
}
