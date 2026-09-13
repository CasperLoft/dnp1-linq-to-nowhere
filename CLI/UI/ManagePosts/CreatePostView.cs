using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;


    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this._postRepository = postRepository;
        this._userRepository = userRepository;
    }

    public async Task StartAsync()
    {
        IQueryable<User> users = _userRepository.GetManyAsync();

        int userId;
        
        while (true)
        {
            string input = CliApp.ReadRequiredInput(
                "Enter your user ID: ");

            if (!int.TryParse(input, out userId))
            {
                Console.WriteLine("User ID must be a number.");
                continue;
            }

            if (!users.Any(user => user.Id == userId))
            {
                Console.WriteLine("No user exists with that ID.");
                continue;
            }

            break;
        }

        string password = CliApp.ReadRequiredInput("Enter your password: ");
        string postTitle = CliApp.ReadRequiredInput("Enter post title: ");
        string postBody = CliApp.ReadRequiredInput("Enter post body: ");

        var newPost = new Post
        {
            UserId = userId,
            Title = postTitle,
            Body = postBody
        };

        Post createdPost = await _postRepository.AddAsync(newPost);

        Console.WriteLine($"Post created with ID: {createdPost.Id}");

        Console.WriteLine();
        Console.WriteLine("Press any key to return...");
        Console.ReadKey();
        
        // TODO Validate user and password logic   
    }
}