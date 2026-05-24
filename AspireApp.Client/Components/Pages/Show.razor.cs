using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Show : ComponentBase
{
    [Inject] public ShowApiClient ShowApi { get; set; } = null!;

    [SupplyParameterFromForm] public Application.Models.App.Show Model { get; set; } = new();

    private List<Application.Models.App.Show> shows = [];
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var result = await ShowApi.GetAllAsync(CancellationToken.None);
        if (result.Success)
            shows = [.. result.Value];
        else
            errorMessage = result.Errors.FormatErrorMessages();
    }

    private async Task HandleRegister()
    {
        var response = await ShowApi.CreateAsync(Model, CancellationToken.None);
        if (response.Success)
        {
            shows.Add(response.Value);
            errorMessage = string.Empty;
            Model = new();
        }
        else
        {
            errorMessage = response.Errors.FormatErrorMessages();
        }
    }
}
