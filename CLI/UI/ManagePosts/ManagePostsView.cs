using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostsView
{
    private readonly IPostRepository _postRepository;

    public ManagePostsView(IPostRepository postRepository)
    {
        this._postRepository = postRepository;
    }
}