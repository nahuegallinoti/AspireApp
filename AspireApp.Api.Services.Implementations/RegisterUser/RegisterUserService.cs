using AspireApp.Application.Contracts.Login;
using AspireApp.Application.Contracts.RegisterUser;
using AspireApp.Core.ROP;
using AspireApp.Entities;

namespace AspireApp.Application.Implementations.RegisterUser;

public class RegisterUserService(IRegisterUserServiceDependencies dependencies) : IRegisterUserService
{
    private readonly IRegisterUserServiceDependencies _registerUserDependencies = dependencies;

    public Task<Result<Usuario>> AddUser(Usuario usuario, CancellationToken cancellationToken = default)
    {
        return ValidateUser(usuario)
        .Bind(VerifyUserDoesntExist)
        .Bind(CreatePasswordHash)
        .Bind(AddUserToDatabase)
        .Map(_ => usuario);
    }

    private static Result<Usuario> ValidateUser(Usuario userAccount)
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
            ? Result.Failure<Usuario>([.. errores])
            : userAccount;
    }

    private Task<Result<Usuario>> VerifyUserDoesntExist(Usuario userAccount) => _registerUserDependencies.VerifyUserDoesNotExist(userAccount);

    private Result<Usuario> CreatePasswordHash(Usuario usuario)
    {
        try
        {
            (byte[] passwordHash, byte[] passwordSalt) = _registerUserDependencies.CreatePasswordHash(usuario.Password);

            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;

            return usuario;
        }

        catch (Exception ex)
        {
            return Result.Failure<Usuario>(ex.Message);
        }
        
    }

    private Task<Result<bool>> AddUserToDatabase(Usuario userAccount) => _registerUserDependencies.AddUser(userAccount);
}
