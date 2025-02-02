﻿using AspireApp.Application.Contracts.Base;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Contracts.Product;

public interface IProductService : IBaseService<Ent.Product, long>
{
}