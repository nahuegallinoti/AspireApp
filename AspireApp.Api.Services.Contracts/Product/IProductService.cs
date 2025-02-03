using AspireApp.Application.Contracts.Base;
using Dto = AspireApp.Api.Domain.Models;

namespace AspireApp.Application.Contracts.Product;

public interface IProductService : IBaseService<Dto.Product, long>
{

}