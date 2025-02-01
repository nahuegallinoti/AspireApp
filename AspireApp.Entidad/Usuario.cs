using AspireApp.Entities.Base;

namespace AspireApp.Entities;

public class Usuario : BaseEntity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
}
