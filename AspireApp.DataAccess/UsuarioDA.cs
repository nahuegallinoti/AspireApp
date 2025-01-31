using AspireApp.DataAccess.Contracts;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entidad;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class UsuarioDA(AppDbContext context) : BaseDA<Usuario>(context), IUsuarioDA
{
    private readonly AppDbContext _context = context;

    public async Task<bool> UserExist(string email) =>
        await _context.Usuarios.AnyAsync(u => u.Email == email);

    public async Task<Usuario?> GetUserByEmail(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
}
