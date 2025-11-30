using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.CreateUserHomeImage
{
    public record CreateUserHomeImageCommand(UserHomeImageRequest Request) : IRequest<Result>;
}
