using CLI.UI;
using DevelopmentData;
using Entities;
using FileRepositories;
using Microsoft.AspNetCore.Identity;
using RepositoryContracts;

string dataDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "data");

IUserRepository userRepository = new UserFileRepository(dataDirectory);
IPostRepository postRepository = new PostFileRepository(dataDirectory);
ICommentRepository commentRepository = new CommentFileRepository(dataDirectory);

if (args.SequenceEqual(["seed", "--reset"]))
{
    var seeder = new DevelopmentDataSeeder(
        userRepository,
        postRepository,
        commentRepository,
        new PasswordHasher<User>());

    await seeder.ResetAndSeedAsync();
    Console.WriteLine("Development data reset complete.");
    Console.WriteLine("Accounts: Alice (Admin), Bob (Moderator), Charlie (User).");
    Console.WriteLine($"Development password: {DevelopmentDataSeeder.DevelopmentPassword}");
    return;
}

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync();

// TODO Username and Password validation
// TODO Optional 1: Manage users: Create new, Update existing, Delete user,
// TODO Optional 2: Manage posts: Create new, Update existing, Delete post
// TODO Optional 3: ”CRUD” operations on the other entities.
// TODO Optional 4: When viewing a list of some entity, consider adding filtering options: See all posts by a specific user id, See all comments a specific user has made, See all users with some specific word in their username.
