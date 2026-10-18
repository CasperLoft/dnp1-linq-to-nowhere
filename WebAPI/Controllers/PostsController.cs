using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PostDTO>>> GetMany(
        [FromQuery] int? userId,
        [FromQuery] string? titleContains,
        [FromQuery] string? userNameContains)
    {
        return Ok(await _postService.GetManyAsync(userId, titleContains, userNameContains));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDTO>> GetById(int id)
    {
        return Ok(await _postService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<PostDTO>> Create([FromBody] CreatePostDTO request)
    {
        PostDTO created = await _postService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDTO>> Update(int id, [FromBody] UpdatePostDTO request)
    {
        return Ok(await _postService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _postService.DeleteAsync(id);
        return NoContent();
    }
}
