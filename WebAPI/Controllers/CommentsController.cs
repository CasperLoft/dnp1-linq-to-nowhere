using DTOs;
using DTOs.Comments.Requests;
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
    public async Task<ActionResult<IReadOnlyCollection<CommentResponse>>> GetMany(
        [FromQuery] int? userId,
        [FromQuery] int? postId,
        [FromQuery] string? userNameContains)
    {
        return Ok(await _commentService.GetManyAsync(userId, postId, userNameContains));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentResponse>> GetById(int id)
    {
        return Ok(await _commentService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create([FromBody] CreateCommentRequest request)
    {
        CommentResponse created = await _commentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentResponse>> Update(int id, [FromBody] UpdateCommentRequest request)
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
