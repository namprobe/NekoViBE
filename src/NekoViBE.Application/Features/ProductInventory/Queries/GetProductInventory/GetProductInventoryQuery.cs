using MediatR;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Queries.GetProductInventory
{
    public record GetProductInventoryQuery(Guid Id) : IRequest<Result<ProductInventoryResponse>>;
}
