using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Microsoft.Extensions.Caching.Hybrid;
using Dto = AspireApp.Api.Models.App;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Implementations.Show;

public class ShowService(IShowDA showDA, ShowMapper mapper, HybridCache hybridCache) : BaseService<Ent.Show, Dto.Show, long>(showDA, mapper, hybridCache), IShowService
{
    //private readonly IShowDA _showDA = showDA;

}