﻿using AspireApp.Core.ROP;
using AspireApp.Entities;

namespace AspireApp.Application.Contracts.Login;

public interface IRegisterUserService : IBaseService
{
    Task<Result<Usuario>> AddUser(Usuario usuario, CancellationToken cancellationToken = default);
}
