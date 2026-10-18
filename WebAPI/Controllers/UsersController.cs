using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDTO>>> GetMany([FromQuery] string? userNameContains)
    {
        return Ok(await _userService.GetManyAsync(userNameContains));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDTO>> GetById(int id)
    {
        return Ok(await _userService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<UserDTO>> Create([FromBody] CreateUserDTO request)
    {
        UserDTO created = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDTO>> Update(int id, [FromBody] UpdateUserDTO request)
    {
        return Ok(await _userService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
