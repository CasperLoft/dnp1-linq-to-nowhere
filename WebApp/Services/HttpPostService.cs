using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using DTOs;
using WebApp.Auth;

namespace WebApp.Services;

public sealed class HttpPostService(HttpClient client, SimpleAuthProvider auth) : IPostService
{
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        string? token = await auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) await auth.ClearSessionAsync();
        return response;
    }

    public async Task<PostResponse> AddPostAsync(CreatePostRequest request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "posts") { Content = JsonContent.Create(request) };
        using HttpResponseMessage response = await SendAsync(message);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"posts/{id}") { Content = JsonContent.Create(request) };
        using HttpResponseMessage response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostResponse>()
               ?? throw new HttpRequestException("The posts API returned an empty response.");
    }

    public async Task DeletePostAsync(int id)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"posts/{id}");
        using HttpResponseMessage response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PostResponse> GetByIdAsync(int id)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"posts/{id}");
        using HttpResponseMessage response = await SendAsync(message);

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
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using HttpResponseMessage response = await SendAsync(message);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"The posts request failed with status code {(int)response.StatusCode}.", null, response.StatusCode);
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
