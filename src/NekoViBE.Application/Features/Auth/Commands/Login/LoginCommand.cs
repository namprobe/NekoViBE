using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : IRequest<Result<AuthResponse>>;