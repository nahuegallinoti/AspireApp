using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Show : ComponentBase
{
    [Inject]
    public ShowApiClient ShowApi { get; set; } = null!;

    [SupplyParameterFromForm]
    public Application.Models.App.Show Model { get; set; } = new();

    private List<Application.Models.App.Show> shows = [];
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ShowApi.GetShowsAsync();

            if (result.Success)
            {
                shows = result.Value.ToList();
            }
            else
            {
                errorMessage = result.Errors.FormatErrorMessages();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error al cargar los shows: {ex.Message}";
        }
    }


    private async Task HandleRegister()
    {
        try
        {
            var response = await ShowApi.CreateShowAsync(Model);

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
