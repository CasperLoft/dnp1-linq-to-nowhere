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

    public Task StartAsync()
    {



        return Task.CompletedTask;
    }

    public Task StartCreateUserView()
    {
        var createUserView = new CreateUserView(_userRepository);
        ClearConsole();
        
        return createUserView.StartAsync();
    }

    public void ShowMenu()
    {
        Console.WriteLine("------ Menu ------");
        Console.WriteLine("1. Create new user:");
        Console.WriteLine("2. Create new post:");
        Console.WriteLine("3. View posts:");

        Console.Write("Enter option 1-3: ");
        string userchoice = Console.ReadLine();

        switch (userchoice)
        {
            case "1":
            {
                StartCreateUserView();
            } ;
                break;
        }
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