using MediatR;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Badge.Command.UpdateBadgeImage
{
    public record UpdateBadgeImageCommand(Guid Id, UpdateBadgeImageRequest Request) : IRequest<Result>;
}
