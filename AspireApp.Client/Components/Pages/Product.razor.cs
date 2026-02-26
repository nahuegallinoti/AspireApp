using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Product : ComponentBase
{
    [Inject]
    public ProductApiClient ProductApi { get; set; } = null!;

    [SupplyParameterFromForm]
    public Application.Models.App.Product Model { get; set; } = new();

    private List<Application.Models.App.Product> products = [];
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ProductApi.GetProductsAsync();

            if (result.Success)
            {
                products = result.Value.ToList();
            }
            else
            {
                errorMessage = result.Errors.FormatErrorMessages();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error al cargar los productos: {ex.Message}";
        }
    }


    private async Task HandleRegister()
    {
        try
        {
            var response = await ProductApi.CreateProductAsync(Model);

            if (response.Success)
            {
                products.Add(response.Value);
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
