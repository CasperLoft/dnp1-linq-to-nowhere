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
    public async Task<ActionResult<IReadOnlyCollection<PostResponse>>> GetMany(
        [FromQuery] int? userId,
        [FromQuery] string? titleContains,
        [FromQuery] string? userNameContains)
    {
        return Ok(await _postService.GetManyAsync(userId, titleContains, userNameContains));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponse>> GetById(int id)
    {
        return Ok(await _postService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<PostResponse>> Create([FromBody] CreatePostRequest request)
    {
        PostResponse created = await _postService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostResponse>> Update(int id, [FromBody] UpdatePostRequest request)
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
