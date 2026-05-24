using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ShowController(IShowService showService, IMessageBus messageBus, ILogger<ShowController> logger)
    : BaseController<Show, long, IShowService>(showService, messageBus, logger);
