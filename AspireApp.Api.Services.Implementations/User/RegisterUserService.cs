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
        .Bind(user => VerifyUserDoesntExist(user, cancellationToken))
        .Bind(AddUserToDatabase);
    }

    private static Result<UserRegister> ValidateUser(UserRegister userAccount)
    {
        List<string> errores = [];

        if (string.IsNullOrWhiteSpace(userAccount.Name))
            errores.Add("El nombre propio no puede estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Surname))
            errores.Add("El apellido propio no puede estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Email))
            errores.Add("El email no debe estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Password))
            errores.Add("El password no debe estar vacio");

        return errores.Count is not 0
            ? Result.Failure<UserRegister>([.. errores])
            : userAccount;
    }

    private Task<Result<UserRegister>> VerifyUserDoesntExist(UserRegister userAccount, CancellationToken cancellationToken = default) =>
        _registerUserDependencies.VerifyUserDoesNotExist(userAccount, cancellationToken);

    private Task<Result<Guid>> AddUserToDatabase(UserRegister usuario) =>
        _registerUserDependencies.AddUser(usuario);
}
