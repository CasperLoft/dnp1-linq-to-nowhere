using DTOs;
using DTOs.Comments.Requests;
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

    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request)
    {
        User author = await EnsureReferencesExistAsync(request.UserId, request.PostId);

        Comment created = await _commentRepository.AddAsync(new Comment
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId
        });

        return ToDto(created, author.UserName);
    }

    public async Task<CommentResponse> GetByIdAsync(int id)
    {
        Comment comment = await GetCommentAsync(id);
        return await ToDtoAsync(comment);
    }

    public Task<IReadOnlyCollection<CommentResponse>> GetManyAsync(
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

        IReadOnlyDictionary<int, string> userNamesById = _userRepository.GetManyAsync()
            .ToDictionary(user => user.Id, user => user.UserName);

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            HashSet<int> matchingUserIds = userNamesById
                .Where(user => user.Value.Contains(userNameContains, StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Key)
                .ToHashSet();

            comments = comments.Where(comment => matchingUserIds.Contains(comment.UserId));
        }

        IReadOnlyCollection<CommentResponse> result = comments
            .Select(comment => ToDto(comment, GetUserName(userNamesById, comment.UserId)))
            .ToArray();
        return Task.FromResult(result);
    }

    public async Task<CommentResponse> UpdateAsync(int id, UpdateCommentRequest request)
    {
        Comment comment = await GetCommentAsync(id);
        User author = await EnsureReferencesExistAsync(comment.UserId, comment.PostId);

        comment.Body = request.Body;
        await _commentRepository.UpdateAsync(comment);

        return ToDto(comment, author.UserName);
    }

    public async Task DeleteAsync(int id)
    {
        await GetCommentAsync(id);
        await _commentRepository.DeleteAsync(id);
    }

    private async Task<CommentResponse> ToDtoAsync(Comment comment)
    {
        User author = await GetUserAsync(comment.UserId);
        return ToDto(comment, author.UserName);
    }

    private async Task<User> EnsureReferencesExistAsync(int userId, int postId)
    {
        User author = await GetUserAsync(userId);

        try
        {
            await _postRepository.GetByIdAsync(postId);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"Post with Id {postId} was not found.", exception);
        }

        return author;
    }

    private async Task<User> GetUserAsync(int userId)
    {
        try { return await _userRepository.GetByIdAsync(userId); }
        catch (InvalidOperationException exception) { throw new ResourceNotFoundException($"User with Id {userId} was not found.", exception); }
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

    private static string GetUserName(IReadOnlyDictionary<int, string> userNamesById, int userId) =>
        userNamesById.TryGetValue(userId, out string? userName)
            ? userName
            : throw new ResourceNotFoundException($"User with Id {userId} was not found.");

    private static CommentResponse ToDto(Comment comment, string userName) =>
        new(comment.Id, comment.Body, comment.UserId, comment.PostId, userName);
}
