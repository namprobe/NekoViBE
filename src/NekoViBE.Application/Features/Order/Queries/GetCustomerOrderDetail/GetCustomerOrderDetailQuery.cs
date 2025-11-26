using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Order.Queries.GetCustomerOrderDetail;

public record GetCustomerOrderDetailQuery(Guid OrderId) : IRequest<Result<CustomerOrderDetailDto>>;

