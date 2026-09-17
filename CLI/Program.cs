using CLI.UI;
using Entities;
using FileRepositories;
using RepositoryContracts;

string dataDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "data");

IUserRepository userRepository = new UserFileRepository(dataDirectory);
IPostRepository postRepository = new PostFileRepository(dataDirectory);
ICommentRepository commentRepository = new CommentFileRepository(dataDirectory);

await SeedDataAsync();

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync();

async Task SeedDataAsync()
{
    if (!userRepository.GetManyAsync().Any())
    {
        var users = new List<User>
        {
            new() { UserName = "Alice", Password = "password1" },
            new() { UserName = "Bob", Password = "password2" },
            new() { UserName = "Charlie", Password = "password3" }
        };

        foreach (User user in users)
        {
            await userRepository.AddAsync(user);
        }
    }

    if (!postRepository.GetManyAsync().Any())
    {
        var posts = new List<Post>
        {
            new Post {Title = "First Post", Body = "This is the first post.", UserId = 1 },
            new Post {Title = "Second Post", Body = "This is the second post.", UserId = 2 },
            new Post {Title = "Third Post", Body = "This is the third post.", UserId = 3 }
        };

        foreach (Post post in posts)
        {
            await postRepository.AddAsync(post);
        }
    }

    if (!commentRepository.GetManyAsync().Any())
    {
        var comments = new List<Comment>
        {
            new Comment {Body = "This is the first comment.", PostId = 1, UserId = 1 },
            new Comment {Body = "This is the second comment.", PostId = 1, UserId = 2 },
            new Comment {Body = "This is the third comment.", PostId = 1, UserId = 3 },

            new Comment {Body = "This is the first comment.", PostId = 2, UserId = 1 },
            new Comment {Body = "This is the second comment.", PostId = 2, UserId = 2 },
            new Comment {Body = "This is the third comment.", PostId = 2, UserId = 3 },

            new Comment {Body = "This is the first comment.", PostId = 3, UserId = 1 },
            new Comment {Body = "This is the second comment.", PostId = 3, UserId = 2 },
            new Comment {Body = "This is the third comment.", PostId = 3, UserId = 3 },
        };

        foreach (Comment comment in comments)
        {
            await commentRepository.AddAsync(comment);
        }
    }
}
// TODO Username and Password validation
// TODO Optional 1: Manage users: Create new, Update existing, Delete user,
// TODO Optional 2: Manage posts: Create new, Update existing, Delete post
// TODO Optional 3: ”CRUD” operations on the other entities.
// TODO Optional 4: When viewing a list of some entity, consider adding filtering options: See all posts by a specific user id, See all comments a specific user has made, See all users with some specific word in their username.