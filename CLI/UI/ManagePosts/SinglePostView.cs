using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public SinglePostView(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task StartAsync(int postId)
    {
        while (true)
        {
            Post post = await _postRepository.GetByIdAsync(postId);
            User postAuthor = await _userRepository.GetByIdAsync(post.UserId);

            Console.WriteLine("--------- Post ---------");
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Author: {postAuthor.UserName}");
            Console.WriteLine();
            Console.WriteLine(post.Body);
            Console.WriteLine();
            Console.WriteLine("------- Comments -------");

            IQueryable<Comment> comments = _commentRepository
                .GetManyAsync()
                .Where(comment => comment.PostId == post.Id);

            if (!comments.Any())
            {
                Console.WriteLine("No comments yet.");
            }
            else
            {
                foreach (Comment comment in comments)
                {
                    User commentAuthor =
                        await _userRepository.GetByIdAsync(comment.UserId);

                    Console.WriteLine($"{commentAuthor.UserName}: {comment.Body}");
                }
            }
            
            Console.WriteLine("-------------------------");
            string userChoice = CliApp.ReadRequiredInput("Press 1 to add comment or 0 to go back: ");

            switch (userChoice)
            {
                case "0":
                {
                    return;
                }

                case "1":
                {
                    CreateCommentView createCommentView = new CreateCommentView(_commentRepository, _userRepository);

                    await createCommentView.StartAsync(postId);
                    continue;
                }

                default:
                    Console.WriteLine("Invalid option. Press any key to try again.");
                    Console.ReadKey();
                    continue;
            }
        }
    }
}