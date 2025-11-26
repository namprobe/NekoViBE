// GetHomeImageListQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.HomeImage.Queries.GetHomeImageList
{
    public record GetHomeImageListQuery(HomeImageFilter Filter) : IRequest<PaginationResult<HomeImageItem>>;
}