using DTOs;

namespace Services;

public interface IPostService
{
    Task<PostDTO> CreateAsync(CreatePostDTO request);
    Task<PostDTO> GetByIdAsync(int id);
    Task<IReadOnlyCollection<PostDTO>> GetManyAsync(int? userId = null, string? titleContains = null);
    Task<PostDTO> UpdateAsync(int id, UpdatePostDTO request);
    Task DeleteAsync(int id);
}
