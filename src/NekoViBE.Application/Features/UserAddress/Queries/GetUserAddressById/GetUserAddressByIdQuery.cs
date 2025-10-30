namespace NekoViBE.Application.Features.UserAddress.Queries.GetUserAddressById;

using MediatR;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Models;

public record GetUserAddressByIdQuery(Guid Id) : IRequest<Result<UserAddressDetail>>;