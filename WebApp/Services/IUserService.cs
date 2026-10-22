using DTOs;

namespace WebApp.Services;

public interface IUserService
{
    public Task<UserDTO> AddUserAsync(CreateUserDTO request);
    public Task UpdateUserAsync(UpdateUserDTO request);
    public Task<UserDTO> GetByIdAsync(int id);
    public Task<IReadOnlyCollection<UserDTO>> GetManyAsync(string? userNameContains = null);
}
