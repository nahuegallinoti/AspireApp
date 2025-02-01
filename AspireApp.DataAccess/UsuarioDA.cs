﻿using AspireApp.DataAccess.Contracts;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class UsuarioDA(AppDbContext context) : BaseDA<User, Guid>(context), IUsuarioDA
{
    private readonly AppDbContext _context = context;

    public async Task<bool> UserExist(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<User?> GetUserByEmail(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
}
