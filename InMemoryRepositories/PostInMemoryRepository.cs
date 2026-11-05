using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository
    : InMemoryRepository<Post>, IPostRepository
{
    public PostInMemoryRepository()
        : base(
        [
            new Post("First Post", "This is the first post.", 1) { Id = 1 },
            new Post("Second Post", "This is the second post.", 2) { Id = 2 },
            new Post("Third Post", "This is the third post.", 3) { Id = 3 }
        ])
    {
    }
}