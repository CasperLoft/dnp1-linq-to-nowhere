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

    public async Task<PostDTO> CreateAsync(CreatePostDTO request)
    {
        await EnsureUserExistsAsync(request.UserId);

        Post created = await _postRepository.AddAsync(new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        });

        return ToDto(created);
    }

    public async Task<PostDTO> GetByIdAsync(int id)
    {
        Post post = await GetPostAsync(id);
        return ToDto(post);
    }

    public Task<IReadOnlyCollection<PostDTO>> GetManyAsync(
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

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            HashSet<int> matchingUserIds = _userRepository.GetManyAsync()
                .Where(user => user.UserName.Contains(userNameContains, StringComparison.OrdinalIgnoreCase))
                .Select(user => user.Id)
                .ToHashSet();

            posts = posts.Where(post => matchingUserIds.Contains(post.UserId));
        }

        IReadOnlyCollection<PostDTO> result = posts.Select(ToDto).ToArray();
        return Task.FromResult(result);
    }

    public async Task<PostDTO> UpdateAsync(int id, UpdatePostDTO request)
    {
        Post post = await GetPostAsync(id);
        await EnsureUserExistsAsync(post.UserId);

        post.Title = request.Title;
        post.Body = request.Body;
        await _postRepository.UpdateAsync(post);

        return ToDto(post);
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

    private async Task EnsureUserExistsAsync(int userId)
    {
        try
        {
            await _userRepository.GetByIdAsync(userId);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"User with Id {userId} was not found.", exception);
        }
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

    private static PostDTO ToDto(Post post) => new(post.Id, post.Title, post.Body, post.UserId);
}
