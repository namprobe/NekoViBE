using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Queries.GetProfile;

public record GetProfileQuery : IRequest<Result<ProfileResponse>>;