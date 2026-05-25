using AspireApp.Application.Models.EventBus;
using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class EventBus : ComponentBase
{
    [SupplyParameterFromForm]
    public EventMessage Model { get; set; } = default!;

    [Inject]
    public ILogger<EventBus> Logger { get; set; } = null!;

    [Inject]
    public MessageBusApiClient Client { get; set; } = null!;

    private string responseMessage = string.Empty;
    private string errorMessage = string.Empty;

    protected override void OnParametersSet()
    {
        Model ??= new EventMessage();
    }

    private async Task SendMessage()
    {
        var result = await Client.SendAsync(Model, CancellationToken.None);

        if (result.Success)
        {
            responseMessage = $"Sent: {result.Value}";
            errorMessage = string.Empty;
        }
        else
        {
            errorMessage = result.Errors.FormatErrorMessages();
            Logger.LogError("Failed to publish event: {Errors}", errorMessage);
        }
    }
}
