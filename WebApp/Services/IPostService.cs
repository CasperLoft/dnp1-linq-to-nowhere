using DTOs;

namespace WebApp.Services;

public interface IPostService
{
    Task<PostResponse> AddPostAsync(CreatePostRequest request);
    Task UpdatePostAsync(UpdatePostRequest request);

    Task<PostResponse> GetByIdAsync(int id);

    Task<IReadOnlyCollection<PostResponse>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null);
}
