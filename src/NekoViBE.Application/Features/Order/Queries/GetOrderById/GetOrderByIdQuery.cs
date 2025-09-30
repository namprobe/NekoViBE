using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Order.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDto>>;

}
