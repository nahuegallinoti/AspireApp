using AspireApp.Application.Models.Auth.User;
using AspireApp.Client.ApiClients;
using AspireApp.Client.Handlers;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] public ILogger<Login> Logger { get; set; } = null!;
    [Inject] public LoginApiClient LoginApi { get; set; } = null!;
    [Inject] public IJwtTokenProvider TokenProvider { get; set; } = null!;

    [SupplyParameterFromForm] private UserLogin Model { get; set; } = new();

    private string errorMessage = string.Empty;

    private async Task HandleLogin()
    {
        errorMessage = string.Empty;

        var result = await LoginApi.LoginAsync(Model, CancellationToken.None);

        if (result.Success)
        {
            TokenProvider.SetToken(result.Value.Token);
            Model = new();
            return;
        }

        errorMessage = result.Errors.FormatErrorMessages();
        Logger.LogWarning("Login failed: {Errors}", errorMessage);
    }
}
