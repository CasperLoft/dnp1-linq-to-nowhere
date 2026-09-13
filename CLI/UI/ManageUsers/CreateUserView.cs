using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository _userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        this._userRepository = userRepository;
    }

    public async Task StartAsync()
    {
        string username = CliApp.ReadRequiredInput("Enter username: ");
        string password = CliApp.ReadRequiredInput("Enter password: ");

        var newuser = new User
        {
            UserName = username,
            Password = password
        };

        User createdUser = await _userRepository.AddAsync(newuser);
        
        Console.WriteLine($"User created with ID: {createdUser.Id}");
        Console.WriteLine();
        Console.WriteLine("Press any key to return...");
        Console.ReadKey();
    }
}
