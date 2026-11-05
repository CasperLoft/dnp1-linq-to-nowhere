using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository
    : InMemoryRepository<User>, IUserRepository
{
    public UserInMemoryRepository()
        : base(
        [
            new User("Alice", "password1") { Id = 1 },
            new User("Bob", "password2") { Id = 2 },
            new User("Charlie", "password3") { Id = 3 }
        ])
    {
    }

    public Task<User?> GetByUsernameAsync(string username) =>
        Task.FromResult(GetManyAsync().FirstOrDefault(user =>
            string.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase)));
}