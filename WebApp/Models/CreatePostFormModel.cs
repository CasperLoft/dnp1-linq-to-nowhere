using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public sealed class CreatePostFormModel
{
    [Required(ErrorMessage = "A title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A post body is required.")]
    public string Body { get; set; } = string.Empty;
}
