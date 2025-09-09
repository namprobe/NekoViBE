using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(ChangePasswordRequest Request) : IRequest<Result>;