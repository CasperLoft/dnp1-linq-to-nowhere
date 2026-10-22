using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IConfiguration config, IAuthService authService) : ControllerBase
{
}