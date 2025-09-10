using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;


namespace NekoViBE.Application.Features.Auth.Queries.GetRole;

public record GetRoleQuery : IRequest<Result<RoleResponse>>;

