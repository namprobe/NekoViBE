using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethods;

public class GetShippingMethodsQueryHandler : IRequestHandler<GetShippingMethodsQuery, PaginationResult<ShippingMethodItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetShippingMethodsQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetShippingMethodsQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ILogger<GetShippingMethodsQueryHandler> logger, 
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<PaginationResult<ShippingMethodItem>> Handle(GetShippingMethodsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user first
            var isValid = (await _currentUserService.IsUserValidAsync()).isValid;
            if (!isValid)
            {
                return PaginationResult<ShippingMethodItem>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            // Build query using extension methods
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false; // Default: giảm dần (newest first)

            // Get paged data from repository
            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending
            );

            // Map to DTOs
            var shippingMethodItems = _mapper.Map<List<ShippingMethodItem>>(items);

            return PaginationResult<ShippingMethodItem>.Success(
                shippingMethodItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping methods with filter: {@Filter}", request.Filter);
            return PaginationResult<ShippingMethodItem>.Failure(
                "Error getting shipping methods", 
                ErrorCodeEnum.InternalError);
        }
    }
}

