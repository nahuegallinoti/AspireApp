using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.EventBus;

public sealed class EventMessage
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Message { get; set; } = string.Empty;
}
