using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository _postRepository;

    public CreatePostView(IPostRepository postRepository)
    {
        this._postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        int userId;

        while (!int.TryParse(
                   CliApp.ReadRequiredInput("Enter your user ID: "),
                   out userId))
        {
            Console.WriteLine("User ID must be a number.");
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