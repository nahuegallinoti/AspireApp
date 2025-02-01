﻿using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Implementations.User;

public class RegisterUserService(IRegisterUserServiceDependencies dependencies) : IRegisterUserService
{
    private readonly IRegisterUserServiceDependencies _registerUserDependencies = dependencies;

    public Task<Result<Guid>> AddUser(UserRegister usuario, CancellationToken cancellationToken = default)
    {
        return ValidateUser(usuario)
        .Bind(VerifyUserDoesntExist)
        .Bind(AddUserToDatabase);
    }

    private static Result<UserRegister> ValidateUser(UserRegister userAccount)
    {
        List<string> errores = [];

        if (string.IsNullOrWhiteSpace(userAccount.Nombre))
            errores.Add("El nombre propio no puede estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Apellido))
            errores.Add("El apellido propio no puede estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Email))
            errores.Add("El email no debe estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Password))
            errores.Add("El password no debe estar vacio");

        return errores.Count is not 0
            ? Result.Failure<UserRegister>([.. errores])
            : userAccount;
    }

    private Task<Result<UserRegister>> VerifyUserDoesntExist(UserRegister userAccount) => _registerUserDependencies.VerifyUserDoesNotExist(userAccount);

    private Task<Result<Guid>> AddUserToDatabase(UserRegister userAccount) => _registerUserDependencies.AddUser(userAccount);
}
