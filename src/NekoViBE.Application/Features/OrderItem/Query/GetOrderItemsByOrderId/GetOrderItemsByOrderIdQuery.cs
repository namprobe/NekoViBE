using MediatR;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.OrderItem.Query.GetOrderItemsByOrderId
{
    public record GetOrderItemsByOrderIdQuery(Guid OrderId) : IRequest<Result<List<OrderItemDetailDTO>>>;
}
