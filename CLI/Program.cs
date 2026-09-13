using CLI.UI;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("Launching LINQ To Nowhere message board...");
IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync();

// TODO Username and Password validation
// TODO Optional 1: Manage users: Create new, Update existing, Delete user, See all users.
// TODO Optional 2: Manage posts: Create new, Update existing, Delete post, See overview of posts e.g. just id and title., view single post.
// TODO Optional 3: ”CRUD” operations on the other entities.
// TODO Optional 4: When viewing a list of some entity, consider adding filtering options: See all posts by a specific user id, See all comments a specific user has made, See all users with some specific word in their username.
