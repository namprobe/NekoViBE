using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.UserAddress.Queries.GetUserAddresses;

public class GetPagedUserAddressQueryHandler : IRequestHandler<GetPagedUserAddressQuery, PaginationResult<UserAddressItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPagedUserAddressQueryHandler> _logger;
    
    public GetPagedUserAddressQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, ILogger<GetPagedUserAddressQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PaginationResult<UserAddressItem>> Handle(GetPagedUserAddressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
                return PaginationResult<UserAddressItem>.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            if (request.Filter.IsCurrentUser is true)
                request.Filter.UserId = userId;
            var filterPredicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var (userAddresses, totalCount) = await _unitOfWork.Repository<Domain.Entities.UserAddress>().GetPagedAsync(
                pageNumber: request.Filter.Page < 1 ? 1 : request.Filter.Page,
                pageSize: request.Filter.PageSize < 1 ? 10 : request.Filter.PageSize,
                predicate: filterPredicate,
                orderBy: orderBy,
                isAscending: request.Filter.IsAscending ?? false // Default: giảm dần (newest first)
            );
            var userAddressItems = _mapper.Map<List<UserAddressItem>>(userAddresses);
            return PaginationResult<UserAddressItem>.Success(userAddressItems, request.Filter.Page, request.Filter.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged user addresses with filter: {@Filter}", request.Filter);
            return PaginationResult<UserAddressItem>.Failure("Error getting paged user addresses", ErrorCodeEnum.InternalError);
        }
    }
}
