namespace DTOs;

public record CommentDTO(
    int Id,
    string Body,
    int UserId,
    int PostId,
    string UserName
);
