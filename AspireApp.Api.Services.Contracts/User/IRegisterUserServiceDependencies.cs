﻿using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserServiceDependencies
{
    Task<Result<UserRegister>> VerifyUserDoesNotExist(UserRegister userAccount, CancellationToken cancellationToken);
    Task<Result<Guid>> AddUser(UserRegister userAccount, CancellationToken cancellationToken);
}