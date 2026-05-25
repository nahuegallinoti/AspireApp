using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Product : ComponentBase
{
    [Inject] public ProductApiClient ProductApi { get; set; } = null!;

    [SupplyParameterFromForm] public Application.Models.App.Product Model { get; set; } = default!;

    private List<Application.Models.App.Product> products = [];
    private string errorMessage = string.Empty;

    protected override void OnParametersSet()
    {
        Model ??= new Application.Models.App.Product();
    }

    protected override async Task OnInitializedAsync()
    {
        var result = await ProductApi.GetAllAsync(CancellationToken.None);
        if (result.Success)
            products = result.Value.ToList();
        else
            errorMessage = result.Errors.FormatErrorMessages();
    }

    private async Task HandleRegister()
    {
        var response = await ProductApi.CreateAsync(Model, CancellationToken.None);
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
}
