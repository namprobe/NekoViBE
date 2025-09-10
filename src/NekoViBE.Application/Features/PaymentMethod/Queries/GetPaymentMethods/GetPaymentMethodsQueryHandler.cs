using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethods;

public class GetPaymentMethodsQueryHandler : IRequestHandler<GetPaymentMethodsQuery, PaginationResult<PaymentMethodItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentMethodsQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentMethodsQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ILogger<GetPaymentMethodsQueryHandler> logger, 
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<PaginationResult<PaymentMethodItem>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user first
            var isValid = (await _currentUserService.IsUserValidAsync()).isValid;
            if (!isValid)
            {
                return PaginationResult<PaymentMethodItem>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            // Build query using extension methods (no QueryBuilder injection needed)
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false; // Default: giảm dần (newest first)

            // Get paged data from repository
            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending
            );

            // Map to DTOs
            var paymentMethodItems = _mapper.Map<List<PaymentMethodItem>>(items);

            return PaginationResult<PaymentMethodItem>.Success(
                paymentMethodItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods with filter: {@Filter}", request.Filter);
            return PaginationResult<PaymentMethodItem>.Failure(
                "Error getting payment methods", 
                ErrorCodeEnum.InternalError);
        }
    }
}