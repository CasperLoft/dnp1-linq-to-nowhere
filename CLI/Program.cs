using CLI.UI;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("Launching LINQ To Nowhere message board...");
IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync();


// TODO 2. Create new post (title, body, user id)
// TODO 3. Add comment to existing post (body, user id, post id)
// TODO 4. View posts overview (just display [title, id] for each post)
// TODO 5. View specific post (see title and body, and comments on the post)
// TODO 6. Optional: Manage users: Create new, Update existing, Delete user, See all users.
// TODO 7. Optional Manage posts: Create new, Update existing, Delete post, See overview of posts e.g. just id and title., view single post.
// TODO 8. Optional: ”CRUD” operations on the other entities.
// TODO 9. Optional: When viewing a list of some entity, consider adding filtering options: See all posts by a specific user id, See all comments a specific user has made, See all users with some specific word in their username.
