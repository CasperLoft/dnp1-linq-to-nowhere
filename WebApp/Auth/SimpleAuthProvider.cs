using System.Security.Claims;
using System.Text.Json;
using DTOs;
using DTOs.Auth.Requests;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace WebApp.Auth;

public class SimpleAuthProvider : AuthenticationStateProvider
{
    private readonly HttpClient httpClient;
    private readonly IJSRuntime _jsRuntime;

    public SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this._jsRuntime = jsRuntime;
    }

    public async Task LoginAsync(string userName, string password)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(userName, password));

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content);
        }

        UserResponse userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        string serialisedData = JsonSerializer.Serialize(userResponse);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUser", serialisedData);
        
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userResponse.UserName),
            new Claim(ClaimTypes.NameIdentifier, userResponse.Id.ToString()),
            new Claim(ClaimTypes.Role, userResponse.Role.ToString()),
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(claimsPrincipal))
            );
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string userAsJson = "";
        try
        {
            userAsJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "currentUser");
        }
        catch (InvalidOperationException e)
        {
            return new AuthenticationState(new());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new());
        }

        UserResponse userResponse = JsonSerializer.Deserialize<UserResponse>(userAsJson)!;
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userResponse.UserName),
            new Claim(ClaimTypes.NameIdentifier, userResponse.Id.ToString()),
            new Claim(ClaimTypes.Role, userResponse.Role.ToString())
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
        
        return new AuthenticationState(claimsPrincipal);
    }

    public async Task Logout()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUser", "");
        NotifyAuthenticationStateChanged(Task.FromResult<AuthenticationState>(new AuthenticationState(new())));
    }
}
