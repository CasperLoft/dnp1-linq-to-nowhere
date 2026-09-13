using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public CliApp(IUserRepository userRepository,
        ICommentRepository commentRepository, IPostRepository postRepository)
    {
        this._userRepository = userRepository;
        this._commentRepository = commentRepository;
        this._postRepository = postRepository;
    }

    public async Task StartAsync()
    {
        bool isRunning = true;

        while (isRunning)
        {
            ClearConsole();
            ShowMenu();

            string? userChoice = Console.ReadLine();
            
            switch (userChoice)
            {
                case "1":
                    await StartCreateUserView();
                    break;
                
                case "2":
                    await StartCreatePostView();
                    break;

                case "3":
                    await StartListPostsView();
                    break;


                case "0":
                    isRunning = false;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private async Task StartListPostsView()
    {
        var listPostView = new ListPostsView(_postRepository);
        ClearConsole();

        int? selectedPostId = listPostView.Start();

        if (selectedPostId is int postId)
        {
            var singlePostView = new SinglePostView(_postRepository, _commentRepository, _userRepository);

            ClearConsole();

            await singlePostView.StartAsync(postId);
        }
    }

    private async Task StartCreatePostView()
    {
        var createPostView = new CreatePostView(_postRepository);
        ClearConsole();

        await createPostView.StartAsync();
    }

    public async Task StartCreateUserView()
    {
        var createUserView = new CreateUserView(_userRepository);
        ClearConsole();
        
        await createUserView.StartAsync();
    }

    public void ShowMenu()
    {
        Console.WriteLine("------ Menu ------");
        Console.WriteLine("1. Create new user:");
        Console.WriteLine("2. Create new post:");
        Console.WriteLine("3. View posts:");
        Console.WriteLine("0. Exit CLI:");

        Console.Write("Enter option 0-3: ");

    }

    public void ClearConsole(){Console.Clear();}
    
    public static string ReadRequiredInput(string prompt)
    {
        // prompt, read, validate, return
        string? input;

        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        }
        while (string.IsNullOrWhiteSpace(input));

        return input;
    }
}