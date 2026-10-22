using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository
    : InMemoryRepository<User>, IUserRepository
{
    public UserInMemoryRepository()
    : base(
    [
        new User { Id = 1, UserName = "Alice" , PasswordHash = "password1" },
        new User { Id = 2, UserName = "Bob" , PasswordHash = "password2" },
        new User { Id = 3, UserName = "Charlie" , PasswordHash = "password3" }
    ])
    {
    }

    public Task<User?> GetByUsernameAsync(string username) =>
        Task.FromResult(GetManyAsync().FirstOrDefault(user =>
            string.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase)));
}
