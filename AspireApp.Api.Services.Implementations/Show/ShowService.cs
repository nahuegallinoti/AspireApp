using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Application.Implementations.Show;

public class ShowService(IShowDA showDA, ShowMapper mapper, HybridCache hybridCache) : BaseService<Domain.Entities.Show, Models.App.Show, long>(showDA, mapper, hybridCache), IShowService
{
    //private readonly IShowDA _showDA = showDA;

}