using AspireApp.Api.Models.Auth.User;
using AspireApp.Client.ApiClients;
using AspireApp.Client.Handlers;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    public ILogger<Login> Logger { get; set; } = null!;

    [Inject]
    public LoginApiClient LoginApi { get; set; } = null!;

    [Inject]
    public JWTTokenProvider TokenProvider { get; set; } = null!;

    [SupplyParameterFromForm]
    private UserLogin Model { get; set; } = new();

    private string errorMessage = string.Empty;

    private async Task HandleLogin()
    {
        CancellationTokenSource cts = new();

        errorMessage = string.Empty;

        try
        {
            var result = await LoginApi.LoginAsync(Model, cts.Token);

            if (result.Success)
            {
                TokenProvider.SetToken(result.Value.Token);
                Model = new();
            }
            else
            {
                errorMessage = result.Errors.FormatErrorMessages();
            }
        }
        catch (OperationCanceledException ex)
        {
            errorMessage = $"La operación fue cancelada: {ex.Message}";
        }
        catch (Exception ex)
        {
            errorMessage = $"Error inesperado: {ex.Message}";
            Logger.LogError(ex, errorMessage);
        }
    }
}
