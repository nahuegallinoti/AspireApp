﻿using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserService
{
    Task<Result<Guid>> AddUser(UserRegister usuario, CancellationToken cancellationToken);
}
