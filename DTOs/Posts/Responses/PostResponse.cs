namespace DTOs;

public record PostResponse(
    int Id,
    string Title,
    string Body,
    int UserId,
    string UserName
);
