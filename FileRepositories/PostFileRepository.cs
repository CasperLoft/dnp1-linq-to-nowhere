using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class PostFileRepository : FileRepository<Post>, IPostRepository
{
    public PostFileRepository(string dataDirectory)
        : base(dataDirectory)
    {
    }
}