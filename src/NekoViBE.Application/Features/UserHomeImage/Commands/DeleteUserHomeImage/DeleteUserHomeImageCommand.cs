// Application/Features/UserHomeImage/Commands/DeleteUserHomeImage/DeleteUserHomeImageCommand.cs
using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.DeleteUserHomeImage
{
    public record DeleteUserHomeImageCommand(Guid Id) : IRequest<Result>;
}