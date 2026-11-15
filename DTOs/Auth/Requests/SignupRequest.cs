using System.ComponentModel.DataAnnotations;

namespace DTOs.Auth.Requests;

public record SignupRequest([Required] string UserName, [Required] string Password);
