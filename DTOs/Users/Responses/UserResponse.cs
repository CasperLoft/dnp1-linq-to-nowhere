using Enums;

namespace DTOs;

public record UserResponse(int Id, string UserName, UserRole Role);