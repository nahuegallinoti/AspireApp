using AspireApp.Application.Contracts.Base;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Application.Contracts.Product;

public interface IProductService : IBaseService<Dto.Product, long>
{

}