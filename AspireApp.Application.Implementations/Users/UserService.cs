using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.Users;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Users;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using UserRoleEntity = AspireApp.Domain.Entities.UserRole;

namespace AspireApp.Application.Implementations.Users;

internal sealed class UserService(
    IUserDA userDA,
    IRoleDA roleDA,
    IRefreshTokenDA refreshTokenDA,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IUserService
{
    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await userDA.GetByIdWithRolesAsync(id, ct);
        return user is null
            ? Result.NotFound<UserDto>("User not found.")
            : UserMapper.ToDto(user).Success();
    }

    public async Task<UserListPage> ListAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var skip = (page - 1) * pageSize;
        var users = await userDA.ListWithRolesAsync(skip, pageSize, search, ct);
        var total = await userDA.CountAsync(search, ct);

        var items = users.Select(UserMapper.ToDto).ToList();
        return new UserListPage(items, total, page, pageSize);
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure<UserDto>(validation.Errors, validation.HttpStatusCode);

        var user = await userDA.GetByIdWithRolesAsync(id, ct);
        if (user is null)
            return Result.NotFound<UserDto>("User not found.");

        user.Name = request.Name.Trim();
        user.Surname = request.Surname.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedUtc = timeProvider.GetUtcNow();

        userDA.Update(user);
        await userDA.SaveChangesAsync(ct);

        return UserMapper.ToDto(user).Success();
    }

    public async Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var user = await userDA.GetByIdAsync(id, ct);
        if (user is null)
            return Result.NotFound<Unit>("User not found.");

        await refreshTokenDA.RevokeAllForUserAsync(user.Id, "UserDeleted", null, ct);

        userDA.Delete(user);
        await userDA.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<UserDto>> AssignRolesAsync(Guid id, AssignRolesRequest request, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure<UserDto>(validation.Errors, validation.HttpStatusCode);

        var user = await userDA.GetByIdWithRolesAsync(id, ct);
        if (user is null)
            return Result.NotFound<UserDto>("User not found.");

        var roles = await roleDA.GetByNamesAsync(request.Roles, ct);
        var missing = request.Roles
            .Where(r => !roles.Any(role => string.Equals(role.Name, r, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (missing.Count > 0)
            return Result.Failure<UserDto>($"Unknown role(s): {string.Join(", ", missing)}.");

        user.UserRoles.Clear();
        foreach (var role in roles)
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = role.Id });

        user.UpdatedUtc = timeProvider.GetUtcNow();
        userDA.Update(user);
        await userDA.SaveChangesAsync(ct);

        await refreshTokenDA.RevokeAllForUserAsync(user.Id, "RolesChanged", null, ct);

        return UserMapper.ToDto(user).Success();
    }

    public async Task<Result<Unit>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure(validation.Errors, validation.HttpStatusCode);

        var user = await userDA.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.NotFound<Unit>("User not found.");

        if (!user.HasPassword)
            return Result.Failure("This account uses single sign-on and has no local password.", System.Net.HttpStatusCode.Conflict);

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash!, user.PasswordSalt!, user.PasswordIterations))
            return Result.Unauthorized<Unit>("Current password is incorrect.");

        var (hash, salt, iterations) = passwordHasher.Hash(request.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.PasswordIterations = iterations;
        user.UpdatedUtc = timeProvider.GetUtcNow();

        userDA.Update(user);
        await userDA.SaveChangesAsync(ct);

        await refreshTokenDA.RevokeAllForUserAsync(user.Id, "PasswordChanged", null, ct);

        return Result.Success();
    }
}
