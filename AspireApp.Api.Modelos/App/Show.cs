using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.App;

public class Show : BaseModel<long>
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}