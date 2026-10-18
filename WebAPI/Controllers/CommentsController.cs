using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CommentDTO>>> GetMany(
        [FromQuery] int? userId,
        [FromQuery] int? postId,
        [FromQuery] string? userNameContains)
    {
        return Ok(await _commentService.GetManyAsync(userId, postId, userNameContains));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDTO>> GetById(int id)
    {
        return Ok(await _commentService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CommentDTO>> Create([FromBody] CreateCommentDTO request)
    {
        CommentDTO created = await _commentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDTO>> Update(int id, [FromBody] UpdateCommentDTO request)
    {
        return Ok(await _commentService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _commentService.DeleteAsync(id);
        return NoContent();
    }
}
