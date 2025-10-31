using MediatR;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserAddress.Commands.CreateUserAddress;

public record CreateUserAddressCommand(UserAddressRequest Request) : IRequest<Result>;