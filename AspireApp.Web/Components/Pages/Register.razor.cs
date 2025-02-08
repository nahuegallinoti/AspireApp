using AspireApp.Api.Domain.Auth.User;
using AspireApp.Web.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Web.Components.Pages;

public partial class Register : ComponentBase
{
    [Inject]
    public RegisterApiClient RegisterApi { get; set; } = null!;

    [SupplyParameterFromForm]
    public UserRegister Model { get; set; } = new();

    private string errorMessage = String.Empty;

    private async Task HandleRegister()
    {
        CancellationTokenSource cts = new();

        errorMessage = String.Empty;

        try
        {
            var result = await RegisterApi.RegisterAsync(Model, cts.Token);

            if (result.Success)
            {
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
            errorMessage = $"Error al registrar: {ex.Message}";
        }
    }

}