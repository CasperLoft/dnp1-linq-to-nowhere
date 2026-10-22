using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : FileRepository<User>, IUserRepository
{
    public UserFileRepository(string dataDirectory)
        : base(dataDirectory)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        List<User> users = await LoadAsync();

        return users.FirstOrDefault(u =>
            u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}
