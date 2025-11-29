// Query
using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImageList
{
    public record GetUserHomeImageListQuery(UserHomeImageFilter Filter) : IRequest<PaginationResult<UserHomeImageItem>>;
}