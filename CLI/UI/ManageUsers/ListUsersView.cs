using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ListUsersView
{
    private readonly IUserRepository _userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        this._userRepository = userRepository;
    }

    public void Start()
    {
        IQueryable<User> users = _userRepository.GetManyAsync();

        Console.WriteLine("        User Overview");
        Console.WriteLine(CliApp.Divider);
        Console.WriteLine(" User ID :     Username");
        Console.WriteLine(CliApp.Divider);

        foreach (User user in users)
        {
            Console.WriteLine($"{user.Id}: {user.UserName}");
        }

        Console.WriteLine(CliApp.Divider);

        while (true)
        {
            string input = CliApp.ReadRequiredInput("Enter 0 to go back: ");

            if (input == "0")
            {
                return;
            }

            Console.WriteLine("Invalid option. Enter 0 to go back.");
        }
    }
}
