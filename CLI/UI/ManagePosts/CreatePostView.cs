using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository _postRepository;

    public CreatePostView(IPostRepository postRepository)
    {
        this._postRepository = postRepository;
    }
}