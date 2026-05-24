using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.App;

public class Product : BaseModel<long>
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}