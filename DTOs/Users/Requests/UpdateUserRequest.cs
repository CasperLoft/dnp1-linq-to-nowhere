using Enums;
using System.ComponentModel.DataAnnotations;

namespace DTOs;

public record UpdateUserRequest(
    [Required] string UserName,
    [EnumDataType(typeof(UserRole))] UserRole Role);
