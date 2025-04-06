using AspireApp.Application.Contracts.Base;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Application.Contracts.Show;

public interface IShowService : IBaseService<Dto.Show, long>
{

}