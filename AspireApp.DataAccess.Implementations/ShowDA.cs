using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.DataAccess.Implementations;

public sealed class ShowDA(AppDbContext context) : BaseDA<Show, long>(context), IShowDA;
