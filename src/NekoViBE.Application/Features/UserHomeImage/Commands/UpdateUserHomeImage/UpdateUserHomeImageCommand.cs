// Application/Features/UserHomeImage/Commands/UpdateUserHomeImage/UpdateUserHomeImageCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.UpdateUserHomeImage
{
    public record UpdateUserHomeImageCommand(Guid Id, UserHomeImageRequest Request) : IRequest<Result>;
}