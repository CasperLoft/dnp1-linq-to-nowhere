using Enums;

namespace DTOs;

public record CreateUserRequest(string UserName, string Password, UserRole Role);
