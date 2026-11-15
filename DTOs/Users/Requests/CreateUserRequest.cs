using Enums;
using System.ComponentModel.DataAnnotations;

namespace DTOs;

public record CreateUserRequest(
    [Required] string UserName,
    [Required] string Password,
    [EnumDataType(typeof(UserRole))] UserRole Role);
