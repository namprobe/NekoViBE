// Query
using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImage
{
    public record GetUserHomeImageQuery(Guid Id) : IRequest<Result<UserHomeImageResponse>>;
}