using Enums;

namespace DTOs;

public record UpdateUserRequest(string UserName, string Password, UserRole Role);
