using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository
    : InMemoryRepository<User>, IUserRepository
{
    public UserInMemoryRepository()
    : base(
    [
        new User { Id = 1, UserName = "Alice" , Password = "password1" },
        new User { Id = 2, UserName = "Bob" , Password = "password2" },
        new User { Id = 3, UserName = "Charlie" , Password = "password3" }
    ])
    {
    }
}
