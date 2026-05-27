using AspireApp.Application.Models.Users;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class UsersApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public sealed record UserListPage(IReadOnlyList<UserDto> Items, int Total, int Page, int PageSize);

    public Task<Result<UserDto>> GetMeAsync(CancellationToken ct) =>
        GetAsync<UserDto>("api/users/me", ct);

    public Task<Result<UserDto>> UpdateMeAsync(UpdateUserProfileRequest request, CancellationToken ct) =>
        PutAsync<UserDto, UpdateUserProfileRequest>("api/users/me", request, ct);

    public Task<Result<Unit>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct) =>
        PostAsync<Unit, ChangePasswordRequest>("api/users/me/change-password", request, ct);

    public Task<Result<UserListPage>> ListAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var url = $"api/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        return GetAsync<UserListPage>(url, ct);
    }

    public Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken ct) =>
        GetAsync<UserDto>($"api/users/{id}", ct);

    public Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct) =>
        PutAsync<UserDto, UpdateUserRequest>($"api/users/{id}", request, ct);

    public Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct) =>
        DeleteAsync<Unit>($"api/users/{id}", ct);

    public Task<Result<UserDto>> AssignRolesAsync(Guid id, AssignRolesRequest request, CancellationToken ct) =>
        PostAsync<UserDto, AssignRolesRequest>($"api/users/{id}/roles", request, ct);
}
