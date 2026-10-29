using DTOs;

namespace WebApp.Services;

public interface IPostService
{
    Task<PostResponse> AddPostAsync(CreatePostRequest request);
    Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request);
    Task DeletePostAsync(int id);

    Task<PostResponse> GetByIdAsync(int id);

    Task<IReadOnlyCollection<PostResponse>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null);
}
