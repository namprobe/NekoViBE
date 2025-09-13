using MediatR;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Queries.GetProduct
{
    public record GetProductQuery(Guid Id) : IRequest<Result<ProductResponse>>;
}
