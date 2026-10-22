using DTOs;

namespace Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> GetByIdAsync(int id);
    Task<IReadOnlyCollection<UserResponse>> GetManyAsync(string? userNameContains = null);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}
