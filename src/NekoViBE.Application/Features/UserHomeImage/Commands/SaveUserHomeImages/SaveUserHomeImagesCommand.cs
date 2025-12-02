// Application/Features/UserHomeImage/Commands/SaveUserHomeImages/SaveUserHomeImagesCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.SaveUserHomeImages
{
    public record SaveUserHomeImagesCommand(List<UserHomeImageSaveRequest> Requests) : IRequest<Result>;
}