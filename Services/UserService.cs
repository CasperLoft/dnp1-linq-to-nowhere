using DTOs;
using Entities;
using RepositoryContracts;

namespace Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public UserService(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<UserDTO> CreateAsync(CreateUserDTO request)
    {
        EnsureUserNameIsAvailable(request.UserName);

        User created = await _userRepository.AddAsync(new User
        {
            UserName = request.UserName,
            Password = request.Password
        });

        return ToDto(created);
    }

    public async Task<UserDTO> GetByIdAsync(int id)
    {
        User user = await GetUserAsync(id);
        return ToDto(user);
    }

    public Task<IReadOnlyCollection<UserDTO>> GetManyAsync(string? userNameContains = null)
    {
        IQueryable<User> users = _userRepository.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            users = users.Where(user => user.UserName.Contains(userNameContains, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyCollection<UserDTO> result = users
            .Select(ToDto)
            .ToArray();

        return Task.FromResult(result);
    }

    public async Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request)
    {
        User user = await GetUserAsync(id);

        if (!string.Equals(user.UserName, request.UserName, StringComparison.OrdinalIgnoreCase))
        {
            EnsureUserNameIsAvailable(request.UserName, id);
        }

        user.UserName = request.UserName;
        user.Password = request.Password;
        await _userRepository.UpdateAsync(user);

        return ToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        await GetUserAsync(id);

        if (_postRepository.GetManyAsync().Any(post => post.UserId == id))
        {
            throw new ConflictException("A user with posts cannot be deleted.");
        }

        if (_commentRepository.GetManyAsync().Any(comment => comment.UserId == id))
        {
            throw new ConflictException("A user with comments cannot be deleted.");
        }

        await _userRepository.DeleteAsync(id);
    }

    private void EnsureUserNameIsAvailable(string userName, int? excludedUserId = null)
    {
        bool exists = _userRepository.GetManyAsync().Any(user =>
            user.Id != excludedUserId &&
            string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new ConflictException("The username is already in use.");
        }
    }

    private async Task<User> GetUserAsync(int id)
    {
        try
        {
            return await _userRepository.GetByIdAsync(id);
        }
        catch (InvalidOperationException exception)
        {
            throw new ResourceNotFoundException($"User with Id {id} was not found.", exception);
        }
    }

    private static UserDTO ToDto(User user) => new(user.Id, user.UserName);
}
