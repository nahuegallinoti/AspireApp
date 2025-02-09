using AspireApp.Api.Models.Rabbit;
using AspireApp.Client;
using AspireApp.Client.Services;
using Microsoft.AspNetCore.Components;

namespace AspireApp.Client.Components.Pages;

public partial class Rabbit
{
    [SupplyParameterFromForm]
    public RabbitMessage Model { get; set; } = new();

    [Inject]
    public ILogger<Rabbit> Logger { get; set; } = null!;

    [Inject]
    public RabbitMqSenderService RabbitMqSender { get; set; } = null!;


    private string responseMessage = "";
    private string errorMessage = "";

    private async Task SendMessage()
    {
        var result = await RabbitMqSender.SendMessageAsync(Model);

        if (result.Success)
        {
            responseMessage = $"Mensaje {result.Value} enviado";
        }
        else
        {
            errorMessage = result.Errors.FormatErrorMessages();
            Logger.LogError(errorMessage);
        }

    }

}