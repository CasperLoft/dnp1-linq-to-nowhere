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
        this._commentRepository = commentRepository;
        this._userRepository = userRepository;
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

            var newComment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Body = commentBody
            };

            await _commentRepository.AddAsync(newComment);
            
            Console.WriteLine("Comment created.");
            return;
        }
    }
}