using AspireApp.Entidad.Base;

namespace AspireApp.Entidad;

public class Usuario : BaseEntity
{
    public string Email { get; set; } = String.Empty;
    public string Nombre { get; set; } = String.Empty;
    public string Apellido { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;

    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
}
