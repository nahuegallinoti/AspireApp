using AspireApp.Application.Models.Roles;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class RolesApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken ct) =>
        GetAsync<IReadOnlyList<RoleDto>>("api/roles", ct);

    public Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken ct) =>
        GetAsync<RoleDto>($"api/roles/{id}", ct);

    public Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct) =>
        PostAsync<RoleDto, CreateRoleRequest>("api/roles", request, ct);

    public Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct) =>
        PutAsync<RoleDto, UpdateRoleRequest>($"api/roles/{id}", request, ct);

    public Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct) =>
        DeleteAsync<Unit>($"api/roles/{id}", ct);
}
