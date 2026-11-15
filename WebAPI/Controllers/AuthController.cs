using System.Security.Claims;
using DTOs;
using DTOs.Auth.Requests;
using DTOs.Auth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using WebAPI.Auth;
using Enums;

namespace WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, SessionStore sessions) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await authService.ValidateUser(request.UserName, request.Password);
        if (user is null) return Unauthorized();
        return Ok(new LoginResponse(sessions.Create(user.Id), new UserResponse(user.Id, user.UserName, user.Role)));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserResponse> Me() => Ok(new UserResponse(
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
        User.Identity!.Name!,
        Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!)));

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        sessions.Remove(Request.Headers.Authorization.ToString()[7..].Trim());
        return NoContent();
    }
}
