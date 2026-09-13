using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ListPostsView
{
    private readonly IPostRepository _postRepository;

    public ListPostsView(IPostRepository postRepository)
    {
        this._postRepository = postRepository;
    }

    public int? Start()
    {
        IQueryable<Post> posts = _postRepository.GetManyAsync();

        Console.WriteLine("        Post Overview");
        Console.WriteLine(CliApp.Divider);
        Console.WriteLine(" Post ID :     Post Title");
        Console.WriteLine(CliApp.Divider);

        foreach (Post post in posts)
        {
            Console.WriteLine($"{post.Id}: {post.Title}");
        }

        Console.WriteLine(CliApp.Divider);

        while (true)
        {
            string input = CliApp.ReadRequiredInput(
                "Enter post ID to view or press 0 to go back: ");

            if (!int.TryParse(input, out int userChoice))
            {
                Console.WriteLine("Post ID must be a number.");
                continue;
            }

            if (userChoice == 0)
            {
                return null;
            }

            if (!posts.Any(post => post.Id == userChoice))
            {
                Console.WriteLine("No post exists with that ID.");
                continue;
            }
            
            // userChoice is now a real post ID.
            return userChoice;
        }
    }
}
