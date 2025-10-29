
using MediatR;
using NekoViBE.Application.Common.Models;
namespace NekoViBE.Application.Features.UserAddress.Commands.DeleteUserAddress;
public record DeleteUserAddressCommand(Guid Id) : IRequest<Result>;