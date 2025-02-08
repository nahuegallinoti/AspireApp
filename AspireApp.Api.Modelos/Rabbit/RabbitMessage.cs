using System.ComponentModel.DataAnnotations;

namespace AspireApp.Api.Domain.Rabbit;

[Serializable]
public class RabbitMessage
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Message { get; set; } = string.Empty;
}