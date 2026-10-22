namespace DTOs;

public record CommentResponse(
    int Id,
    string Body,
    int UserId,
    int PostId,
    string UserName
);
