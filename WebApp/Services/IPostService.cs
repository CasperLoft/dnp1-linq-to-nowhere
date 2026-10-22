using DTOs;

namespace WebApp.Services;

public interface IPostService
{
    Task<PostDTO> AddPostAsync(CreatePostDTO request);
    Task UpdatePostAsync(UpdatePostDTO request);

    Task<IReadOnlyCollection<PostDTO>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null);
}
