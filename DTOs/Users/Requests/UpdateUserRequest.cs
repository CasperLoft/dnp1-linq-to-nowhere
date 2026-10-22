using Enums;

namespace DTOs;

public record UpdateUserRequest(string UserName, UserRole Role);
