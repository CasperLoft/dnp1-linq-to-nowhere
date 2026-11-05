using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreateCommentView
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public CreateCommentView(ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task StartAsync(int postId)
    {
        IQueryable<User> users = _userRepository.GetManyAsync();

        while (true)
        {
            string input = CliApp.ReadRequiredInput(
                "Enter your user ID: ");

            if (!int.TryParse(input, out int userId))
            {
                Console.WriteLine("User ID must be a number.");
                continue;
            }

            if (!users.Any(user => user.Id == userId))
            {
                Console.WriteLine("No user exists with that ID.");
                continue;
            }

            string commentBody = CliApp.ReadRequiredInput("Write your comment: ");

            // Using the parameterized constructor matching Comment(string body, int postId, int userId)
            var newComment = new Comment(commentBody, postId, userId);

            await _commentRepository.AddAsync(newComment);
            
            Console.WriteLine("Comment created.");
            return;
        }
    }
}