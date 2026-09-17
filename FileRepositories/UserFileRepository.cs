using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : FileRepository<User>, IUserRepository
{
    public UserFileRepository(string dataDirectory)
        : base(dataDirectory)
    {
    }
}