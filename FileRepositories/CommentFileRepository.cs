using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class CommentFileRepository : FileRepository<Comment>, ICommentRepository
{
    public CommentFileRepository(string dataDirectory)
        : base(dataDirectory)
    {
    }
}