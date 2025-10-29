
using MediatR;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserAddress.Commands.UpdateUserAddress;

public record UpdateUserAddressCommand(Guid Id, UserAddressRequest Request) : IRequest<Result>;
