using Entities;

namespace RepositoryContracts;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int userId);
    Task<User> GetByIdAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    IQueryable<User> GetManyAsync();
}
