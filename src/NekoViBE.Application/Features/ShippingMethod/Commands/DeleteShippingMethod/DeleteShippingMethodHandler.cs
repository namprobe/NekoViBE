using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.DeleteShippingMethod;

public class DeleteShippingMethodHandler : IRequestHandler<DeleteShippingMethodCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteShippingMethodHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DeleteShippingMethodHandler(IUnitOfWork unitOfWork, ILogger<DeleteShippingMethodHandler> logger, 
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteShippingMethodCommand request, CancellationToken cancellationToken)
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
                return Result.Failure("User is not allowed to delete shipping method", ErrorCodeEnum.Forbidden);
            }
            
            var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (shippingMethod == null)
            {
                return Result.Failure("Shipping method not found", ErrorCodeEnum.NotFound);
            }

            // Check if shipping method is in use
            var isInUse = await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>()
                .AnyAsync(x => x.ShippingMethodId == request.Id);
            if (isInUse)
            {
                return Result.Failure("Shipping method is in use and cannot be deleted", ErrorCodeEnum.ValidationFailed);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                _unitOfWork.Repository<Domain.Entities.ShippingMethod>().Delete(shippingMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Shipping method deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipping method");
            return Result.Failure("Error deleting shipping method", ErrorCodeEnum.InternalError);
        }
    }
}
