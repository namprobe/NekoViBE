using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Result>;