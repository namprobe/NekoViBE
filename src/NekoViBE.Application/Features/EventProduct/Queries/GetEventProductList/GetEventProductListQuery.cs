using MediatR;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Queries.GetEventProductList
{
    public record GetEventProductListQuery(EventProductFilter Filter) : IRequest<PaginationResult<EventProductItem>>;
}
