namespace DTOs;

public record PostDTO(
    int Id,
    string Title,
    string Body,
    int UserId,
    string UserName
);
