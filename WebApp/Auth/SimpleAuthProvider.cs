using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using DTOs;
using DTOs.Auth.Requests;
using DTOs.Auth.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace WebApp.Auth;

public class SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private const string TokenKey = "sessionToken";
    private static AuthenticationState Anonymous => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static AuthenticationState FromUser(UserResponse user) => new(new ClaimsPrincipal(new ClaimsIdentity([
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    ], "api-session")));

    public async Task<string?> GetTokenAsync() => await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);

    public async Task LoginAsync(string userName, string password)
    {
        using var response = await httpClient.PostAsJsonAsync("auth/login", new LoginRequest(userName, password));
        if (!response.IsSuccessStatusCode) throw new HttpRequestException("Login failed. Check your username and password.");
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new HttpRequestException("Empty login response.");
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "currentUser");
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenKey, login.Token);
        NotifyAuthenticationStateChanged(Task.FromResult(FromUser(login.User)));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return Anonymous;
            using var request = new HttpRequestMessage(HttpMethod.Get, "auth/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await ClearSessionAsync();
                return Anonymous;
            }
            if (!response.IsSuccessStatusCode) return Anonymous;
            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            return user is null ? Anonymous : FromUser(user);
        }
        catch (InvalidOperationException) { return Anonymous; }
        catch (JSException) { return Anonymous; }
        catch (HttpRequestException) { return Anonymous; }
    }

    public async Task ClearSessionAsync()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public Task RefreshAsync()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public async Task Logout()
    {
        string? token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await httpClient.SendAsync(request);
            }
            catch (HttpRequestException) { }
        }
        await ClearSessionAsync();
    }
}
