using DTOs;

namespace WebApp.Services;

public interface IUserService
{
    public Task<UserResponse> AddUserAsync(CreateUserRequest request);
    public Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
    public Task<UserResponse> GetByIdAsync(int id);
    public Task<IReadOnlyCollection<UserResponse>> GetManyAsync(string? userNameContains = null);
}
