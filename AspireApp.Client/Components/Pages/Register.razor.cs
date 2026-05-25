using AspireApp.Application.Models.Auth.User;
using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Register : ComponentBase
{
    [Inject] public RegisterApiClient RegisterApi { get; set; } = null!;

    [SupplyParameterFromForm] public UserRegister Model { get; set; } = default!;

    private string errorMessage = string.Empty;

    protected override void OnParametersSet()
    {
        Model ??= new UserRegister();
    }

    private async Task HandleRegister()
    {
        errorMessage = string.Empty;
        var result = await RegisterApi.RegisterAsync(Model, CancellationToken.None);

        if (result.Success)
            Model = new();
        else
            errorMessage = result.Errors.FormatErrorMessages();
    }
}
