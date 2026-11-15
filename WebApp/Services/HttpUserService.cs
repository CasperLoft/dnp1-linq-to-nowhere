using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DTOs;
using WebApp.Auth;

namespace WebApp.Services;

public sealed class HttpUserService(HttpClient client, SimpleAuthProvider auth) : IUserService
{
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        string? token = await auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) await auth.ClearSessionAsync();
        else if (response.StatusCode == HttpStatusCode.Forbidden) await auth.RefreshAsync();
        return response;
    }

    public async Task<UserResponse> AddUserAsync(CreateUserRequest request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "users") { Content = JsonContent.Create(request) };
        using var response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>() ?? throw new HttpRequestException("The users API returned an empty response.");
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"users/{id}");
        using var response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>() ?? throw new HttpRequestException("The users API returned an empty response.");
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetManyAsync(string? userNameContains = null)
    {
        string uri = string.IsNullOrWhiteSpace(userNameContains) ? "users" : $"users?userNameContains={Uri.EscapeDataString(userNameContains)}";
        using var message = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse[]>() ?? [];
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"users/{id}") { Content = JsonContent.Create(request) };
        using var response = await SendAsync(message);
        response.EnsureSuccessStatusCode();
        await auth.RefreshAsync();
        return await response.Content.ReadFromJsonAsync<UserResponse>() ?? throw new HttpRequestException("The users API returned an empty response.");
    }
}
