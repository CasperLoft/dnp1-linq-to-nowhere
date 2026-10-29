using System.Net.Http.Json;
using DTOs;

namespace WebApp.Services;

public sealed class HttpPostService(HttpClient client) : IPostService
{
    public async Task<PostResponse> AddPostAsync(CreatePostRequest request)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("posts", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request)
    {
        using HttpResponseMessage response = await client.PutAsJsonAsync($"posts/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task DeletePostAsync(int id)
    {
        using HttpResponseMessage response = await client.DeleteAsync($"posts/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<PostResponse> GetByIdAsync(int id)
    {
        using HttpResponseMessage response = await client.GetAsync($"posts/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"The post request failed with status code {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task<IReadOnlyCollection<PostResponse>> GetManyAsync(
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

        return await response.Content.ReadFromJsonAsync<PostResponse[]>() ?? [];
    }

    private static void AddQueryValue(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}
