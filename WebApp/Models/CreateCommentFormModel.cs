using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public sealed class CreateCommentFormModel
{
    [Required(ErrorMessage = "A comment is required.")]
    public string Body { get; set; } = string.Empty;
}
