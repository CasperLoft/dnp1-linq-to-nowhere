using DTOs;

namespace Services;

public interface IUserService
{
    Task<UserDTO> CreateAsync(CreateUserDTO request);
    Task<UserDTO> GetByIdAsync(int id);
    Task<IReadOnlyCollection<UserDTO>> GetManyAsync();
    Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request);
    Task DeleteAsync(int id);
}
