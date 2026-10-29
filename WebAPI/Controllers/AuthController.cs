using DTOs;
using DTOs.Auth.Requests;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
    {
        User? user = await _authService.ValidateUser(request.UserName, request.Password);

        if (user is null)
        {
            return Unauthorized(); // HTTP 401
        }

        UserResponse response = new UserResponse(user.Id, user.UserName, user.Role);
        
        return Ok(response);
    }
}