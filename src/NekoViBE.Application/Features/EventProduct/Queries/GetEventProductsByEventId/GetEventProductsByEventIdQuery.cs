// File: Application/Features/EventProduct/Queries/GetEventProductsByEventId/GetEventProductsByEventIdQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.EventProduct.Queries.GetEventProductsByEventId
{
    public record GetEventProductsByEventIdQuery(Guid EventId)
        : IRequest<Result<List<EventProductWithProductItem>>>;
}