using System.Net.Http.Json;
using DTOs;

namespace WebApp.Services;

public sealed class HttpPostService(HttpClient client) : IPostService
{
    public async Task<PostDTO> AddPostAsync(CreatePostDTO request)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("posts", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PostDTO>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public Task UpdatePostAsync(UpdatePostDTO request)
    {
        throw new NotSupportedException("Updating a post requires its ID, which is not part of UpdatePostDTO.");
    }

    public async Task<PostDTO> GetByIdAsync(int id)
    {
        using HttpResponseMessage response = await client.GetAsync($"posts/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"The post request failed with status code {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<PostDTO>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task<IReadOnlyCollection<PostDTO>> GetManyAsync(
        int? userId = null,
        string? titleContains = null,
        string? userNameContains = null)
    {
        var query = new List<string>();

        if (userId.HasValue)
        {
            query.Add($"userId={userId.Value}");
        }

        AddQueryValue(query, "titleContains", titleContains);
        AddQueryValue(query, "userNameContains", userNameContains);

        string requestUri = query.Count == 0 ? "posts" : $"posts?{string.Join("&", query)}";
        using HttpResponseMessage response = await client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"The posts request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<PostDTO[]>() ?? [];
    }

    private static void AddQueryValue(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}
