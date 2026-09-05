using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository
    : InMemoryRepository<Post>, IPostRepository
{
    public PostInMemoryRepository()
    : base(
    [
        new Post { Id = 1, Title = "First Post", Body = "This is the first post.", UserId = 1 },
        new Post { Id = 2, Title = "Second Post", Body = "This is the second post.", UserId = 2 },
        new Post { Id = 3, Title = "Third Post", Body = "This is the third post.", UserId = 3 }
    ])
    {
    }
}