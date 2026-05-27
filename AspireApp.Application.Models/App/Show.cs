using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.App;

public sealed class Show : BaseModel<long>
{
    [Required(ErrorMessage = "Field {0} is required.")]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}
