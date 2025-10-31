using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Order.Queries.GetOrderList
{
    public record GetOrderListQuery(OrderFilter Filter) : IRequest<PaginationResult<OrderListItem>>;
}
