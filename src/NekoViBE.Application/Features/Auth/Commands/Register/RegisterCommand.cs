using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<Result>;

