using DTOs;

namespace WebApp.Services;

public sealed class HttpCommentService(HttpClient client) : ICommentService
{
    public async Task<CommentDTO> AddCommentAsync(CreateCommentDTO request)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("comments", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CommentDTO>()
               ?? throw new HttpRequestException("The comments API returned an empty response.");
    }

    public Task UpdateCommentAsync(UpdateCommentDTO request)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<CommentDTO>> GetManyAsync(int? postId = null, int? userId = null)
    {
        var commentsList = new List<string>();

        if (postId.HasValue)
        {
            commentsList.Add($"postId={postId.Value}");
        }

        if (userId.HasValue)
        {
            commentsList.Add($"userId={userId.Value}");
        }

        string requestUri = commentsList.Count == 0 ? "comments" : $"comments?{string.Join("&", commentsList)}";
        using HttpResponseMessage response = await client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"The comments request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<CommentDTO[]>() ?? [];
    }
}
