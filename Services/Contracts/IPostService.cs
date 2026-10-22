using DTOs;

namespace Services;

public interface IPostService
{
    Task<PostResponse> CreateAsync(CreatePostRequest request);
    Task<PostResponse> GetByIdAsync(int id);
    Task<IReadOnlyCollection<PostResponse>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null);
    Task<PostResponse> UpdateAsync(int id, UpdatePostRequest request);
    Task DeleteAsync(int id);
}
