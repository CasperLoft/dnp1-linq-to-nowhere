namespace DTOs.Comments.Requests;

public record CreateCommentRequest(string Body, int UserId, int PostId);
