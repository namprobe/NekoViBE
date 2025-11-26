using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Order.Queries.GetCustomerOrderList;

public record GetCustomerOrderListQuery(OrderFilter Filter) : IRequest<PaginationResult<CustomerOrderListItem>>;

