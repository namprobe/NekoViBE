using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethod;

public class GetPaymentMethodQueryHandler : IRequestHandler<GetPaymentMethodQuery, Result<PaymentMethodResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentMethodQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentMethodQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetPaymentMethodQueryHandler> logger, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaymentMethodResponse>> Handle(GetPaymentMethodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid)
            {
                return Result<PaymentMethodResponse>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            var paymentMethod = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (paymentMethod == null)
            {
                return Result<PaymentMethodResponse>.Failure("Payment method not found", ErrorCodeEnum.NotFound);
            }
            var paymentMethodResponse = _mapper.Map<PaymentMethodResponse>(paymentMethod);
            return Result<PaymentMethodResponse>.Success(paymentMethodResponse, "Payment method retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method");
            return Result<PaymentMethodResponse>.Failure("Error getting payment method", ErrorCodeEnum.InternalError);
        }
    }
}