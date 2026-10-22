using Entities;
using Microsoft.AspNetCore.Identity;
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

        var newUser = new User
        {
            UserName = username
        };

        newUser.PasswordHash = new PasswordHasher<User>().HashPassword(newUser, password);

        User createdUser = await _userRepository.AddAsync(newUser);

        Console.WriteLine($"User created with ID: {createdUser.Id}");
        Console.WriteLine();
        Console.WriteLine("Press any key to return...");
        Console.ReadKey();
    }
}
