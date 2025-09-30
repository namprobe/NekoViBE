using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(UpdateProfileRequest UpdateProfileRequest) : IRequest<Result>;