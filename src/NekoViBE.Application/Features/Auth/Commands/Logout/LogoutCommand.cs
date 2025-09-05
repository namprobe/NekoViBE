using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>;