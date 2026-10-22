using System.ComponentModel.DataAnnotations;
using Enums;

namespace WebApp.Models;

public sealed class CreateUserFormModel
{
    [Required(ErrorMessage = "A username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "A password is required.")]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;
}
