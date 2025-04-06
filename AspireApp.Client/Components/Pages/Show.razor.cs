using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Client.Components.Pages;

public partial class Show : ComponentBase
{
    [Inject]
    public ShowApiClient ShowApi { get; set; } = null!;

    [SupplyParameterFromForm]
    public Dto.Show Model { get; set; } = new();

    private List<Dto.Show> shows = [];
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
