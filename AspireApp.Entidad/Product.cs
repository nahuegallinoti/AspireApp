using AspireApp.Entities.Base;

namespace AspireApp.Entities;

public class Product : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
