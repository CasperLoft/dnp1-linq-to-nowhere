using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository
    : InMemoryRepository<Comment>, ICommentRepository
{
    public CommentInMemoryRepository()
        : base(
        [
            new Comment("This is the first comment.", 1, 1) { Id = 1 },
            new Comment("This is the second comment.", 1, 2) { Id = 2 },
            new Comment("This is the third comment.", 1, 3) { Id = 3 },

            new Comment("This is the first comment.", 2, 1) { Id = 4 },
            new Comment("This is the second comment.", 2, 2) { Id = 5 },
            new Comment("This is the third comment.", 2, 3) { Id = 6 },

            new Comment("This is the first comment.", 3, 1) { Id = 7 },
            new Comment("This is the second comment.", 3, 2) { Id = 8 },
            new Comment("This is the third comment.", 3, 3) { Id = 9 },
        ])
    {
    }
}