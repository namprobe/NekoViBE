using MediatR;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserAddress.Queries.GetUserAddresses;

public record GetPagedUserAddressQuery(UserAddressFilter Filter) : IRequest<PaginationResult<UserAddressItem>>;