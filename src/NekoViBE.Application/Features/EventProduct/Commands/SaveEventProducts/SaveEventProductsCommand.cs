// Application/Features/Event/Commands/SaveEventProducts/SaveEventProductsCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Event.Commands.SaveEventProducts
{
    public record SaveEventProductsCommand(Guid EventId, List<EventProductSaveRequest> Requests) : IRequest<Result>;
}