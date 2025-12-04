// Application/Features/EventProduct/Commands/UpdateEventProductList/UpdateEventProductListCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProductList
{
    public record UpdateEventProductListCommand(Guid EventId, List<EventProductItemRequest> Products) : IRequest<Result>;
}