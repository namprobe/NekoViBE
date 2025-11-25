using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.DeletePayment;

public class DeletePaymentHandler : IRequestHandler<DeletePaymentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePaymentHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DeletePaymentHandler(IUnitOfWork unitOfWork, ILogger<DeletePaymentHandler> logger, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
            {
                return Result.Failure("User is not allowed to delete payment method", ErrorCodeEnum.Forbidden);
            }
            var paymentMethod = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (paymentMethod == null)
            {
                return Result.Failure("Payment method not found", ErrorCodeEnum.NotFound);
            }

            if (await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().AnyAsync(x => x.Id == request.Id))
            {
                return Result.Failure("Payment method is in use", ErrorCodeEnum.ValidationFailed);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                _unitOfWork.Repository<Domain.Entities.PaymentMethod>().Delete(paymentMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Payment method deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method");
            return Result.Failure("Error deleting payment method", ErrorCodeEnum.InternalError);
        }
    }
}