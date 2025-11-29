using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethod;

public class GetShippingMethodQueryHandler : IRequestHandler<GetShippingMethodQuery, Result<ShippingMethodResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetShippingMethodQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetShippingMethodQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetShippingMethodQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ShippingMethodResponse>> Handle(GetShippingMethodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            
            var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == request.Id && x.Status == EntityStatusEnum.Active);

            if (shippingMethod == null)
            {
                return Result<ShippingMethodResponse>.Failure("Shipping method not found", ErrorCodeEnum.NotFound);
            }

            var response = _mapper.Map<ShippingMethodResponse>(shippingMethod);

            return Result<ShippingMethodResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping method with ID: {Id}", request.Id);
            return Result<ShippingMethodResponse>.Failure(
                "Error getting shipping method",
                ErrorCodeEnum.InternalError);
        }
    }
}

