using Entities;
using Microsoft.AspNetCore.Identity;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository _userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task StartAsync()
    {
        string username = CliApp.ReadRequiredInput("Enter username: ");
        string password = CliApp.ReadRequiredInput("Enter password: ");

        // 1. Hash the password before instantiating the user
        var passwordHasher = new PasswordHasher<User>();
        string passwordHash = passwordHasher.HashPassword(null!, password);

        // 2. Instantiate User using the parameterized constructor
        var newUser = new User(username, passwordHash);

        User createdUser = await _userRepository.AddAsync(newUser);

        Console.WriteLine($"User created with ID: {createdUser.Id}");
        Console.WriteLine();
        Console.WriteLine("Press any key to return...");
        Console.ReadKey();
    }
}