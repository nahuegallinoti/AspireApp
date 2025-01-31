namespace AspireApp.Entidad.Base;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}