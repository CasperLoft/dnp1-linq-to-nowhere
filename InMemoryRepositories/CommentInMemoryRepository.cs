using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository
    : InMemoryRepository<Comment>, ICommentRepository
{
    public CommentInMemoryRepository()
    : base(
    [
        new Comment { Id = 1, Body = "This is the first comment.", PostId = 1, UserId = 1 },
        new Comment { Id = 2, Body = "This is the second comment.", PostId = 1, UserId = 2 },
        new Comment { Id = 3, Body = "This is the third comment.", PostId = 1, UserId = 3 },

        new Comment { Id = 1, Body = "This is the first comment.", PostId = 2, UserId = 1 },
        new Comment { Id = 2, Body = "This is the second comment.", PostId = 2, UserId = 2 },
        new Comment { Id = 3, Body = "This is the third comment.", PostId = 2, UserId = 3 },

        new Comment { Id = 1, Body = "This is the first comment.", PostId = 3, UserId = 1 },
        new Comment { Id = 2, Body = "This is the second comment.", PostId = 3, UserId = 2 },
        new Comment { Id = 3, Body = "This is the third comment.", PostId = 3, UserId = 3 },

    ])
    {
    }
}
