using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository _postRepository;

    public SinglePostView(IPostRepository postRepository)
    {
        this._postRepository = postRepository;
    }
}