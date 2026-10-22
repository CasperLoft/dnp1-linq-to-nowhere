using DTOs;
using Entities;
using RepositoryContracts;

namespace Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
    }

    public async Task<PostResponse> CreateAsync(CreatePostRequest request)
    {
        User author = await GetUserAsync(request.UserId);

        Post created = await _postRepository.AddAsync(new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        });

        return ToDto(created, author.UserName);
    }

    public async Task<PostResponse> GetByIdAsync(int id)
    {
        Post post = await GetPostAsync(id);
        return await ToDtoAsync(post);
    }

    public Task<IReadOnlyCollection<PostResponse>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null)
    {
        IQueryable<Post> posts = _postRepository.GetManyAsync();

        if (userId.HasValue)
        {
            posts = posts.Where(post => post.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            posts = posts.Where(post => post.Title.Contains(titleContains, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyDictionary<int, string> userNamesById = _userRepository.GetManyAsync()
            .ToDictionary(user => user.Id, user => user.UserName);

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            HashSet<int> matchingUserIds = userNamesById
                .Where(user => user.Value.Contains(userNameContains, StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Key)
                .ToHashSet();

            posts = posts.Where(post => matchingUserIds.Contains(post.UserId));
        }

        IReadOnlyCollection<PostResponse> result = posts
            .Select(post => ToDto(post, GetUserName(userNamesById, post.UserId)))
            .ToArray();
        return Task.FromResult(result);
    }

    public async Task<PostResponse> UpdateAsync(int id, UpdatePostRequest request)
    {
        Post post = await GetPostAsync(id);
        User author = await GetUserAsync(post.UserId);

        post.Title = request.Title;
        post.Body = request.Body;
        await _postRepository.UpdateAsync(post);

        return ToDto(post, author.UserName);
    }

    public async Task DeleteAsync(int id)
    {
        await GetPostAsync(id);

        if (_commentRepository.GetManyAsync().Any(comment => comment.PostId == id))
        {
            throw new ConflictException("A post with comments cannot be deleted.");
        }

        await _postRepository.DeleteAsync(id);
    }

    private async Task<PostResponse> ToDtoAsync(Post post)
    {
        User author = await GetUserAsync(post.UserId);
        return ToDto(post, author.UserName);
    }

    private async Task<User> GetUserAsync(int userId)
    {
        try { return await _userRepository.GetByIdAsync(userId); }
        catch (InvalidOperationException exception) { throw new ResourceNotFoundException($"User with Id {userId} was not found.", exception); }
    }

    private async Task<Post> GetPostAsync(int id)
    {
        try
        {
            return await _postRepository.GetByIdAsync(id);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"Post with Id {id} was not found.", exception);
        }
    }

    private static string GetUserName(IReadOnlyDictionary<int, string> userNamesById, int userId) =>
        userNamesById.TryGetValue(userId, out string? userName)
            ? userName
            : throw new ResourceNotFoundException($"User with Id {userId} was not found.");

    private static PostResponse ToDto(Post post, string userName) =>
        new(post.Id, post.Title, post.Body, post.UserId, userName);
}
