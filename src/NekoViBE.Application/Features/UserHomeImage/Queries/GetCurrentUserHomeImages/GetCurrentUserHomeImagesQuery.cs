// File: Application/Features/UserHomeImage/Queries/GetCurrentUserHomeImages/GetCurrentUserHomeImagesQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetCurrentUserHomeImages
{
    public record GetCurrentUserHomeImagesQuery : IRequest<Result<List<UserHomeImageItem>>>;
}