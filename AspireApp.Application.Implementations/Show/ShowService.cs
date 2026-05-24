using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using ShowEntity = AspireApp.Domain.Entities.Show;
using ShowModel = AspireApp.Application.Models.App.Show;

namespace AspireApp.Application.Implementations.Show;

internal sealed class ShowService(IShowDA showDA, ShowMapper mapper, HybridCache hybridCache)
    : BaseService<ShowEntity, ShowModel, long>(showDA, mapper, hybridCache), IShowService;
