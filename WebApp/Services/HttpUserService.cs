using System.Net.Http.Json;
using DTOs;

namespace WebApp.Services;

public sealed class HttpUserService(HttpClient client) : IUserService
{
    public async Task<UserResponse> AddUserAsync(CreateUserRequest request)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("users", request);
        EnsureSuccess(response, "create");

        return await response.Content.ReadFromJsonAsync<UserResponse>()
               ?? throw new HttpRequestException("The users API returned an empty response.");
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        using HttpResponseMessage response = await client.GetAsync($"users/{id}");
        EnsureSuccess(response, "request");

        return await response.Content.ReadFromJsonAsync<UserResponse>()
               ?? throw new HttpRequestException("The users API returned an empty response.");
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetManyAsync(string? userNameContains = null)
    {
        string requestUri = string.IsNullOrWhiteSpace(userNameContains)
            ? "users"
            : $"users?userNameContains={Uri.EscapeDataString(userNameContains)}";
        using HttpResponseMessage response = await client.GetAsync(requestUri);
        EnsureSuccess(response, "request");

        return await response.Content.ReadFromJsonAsync<UserResponse[]>() ?? [];
    }

    public Task UpdateUserAsync(UpdateUserRequest request)
    {
        throw new NotSupportedException("Updating a user requires its ID, which is not part of UpdateUserDTO.");
    }

    private static void EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"The user {operation} failed with status code {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }
    }
}
