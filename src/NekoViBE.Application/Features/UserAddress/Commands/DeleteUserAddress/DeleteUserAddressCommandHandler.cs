using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserAddress.Commands.DeleteUserAddress;

public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserAddressCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public DeleteUserAddressCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService, 
        ILogger<DeleteUserAddressCommandHandler> logger, 
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Result> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
                return Result.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
                
            var isCustomer = await _currentUserService.HasRoleAsync(RoleEnum.Customer);
            if (!isCustomer)
            {
                return Result.Failure("User is not a customer", ErrorCodeEnum.Forbidden);
            }
            
            var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (userAddress is null)
            {
                return Result.Failure("User address not found", ErrorCodeEnum.NotFound);
            }
            
            userAddress.SoftDeleteEnitity(userId);
            _unitOfWork.Repository<Domain.Entities.UserAddress>().Update(userAddress);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Log user action using helper (fire and forget)
            UserActionHelper.LogDeleteAction<Domain.Entities.UserAddress>(
                _serviceProvider,
                userId.Value,
                userAddress.Id,
                _currentUserService.IPAddress,
                cancellationToken
            );
            
            return Result.Success("User address deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user address: {@Request}", request);
            return Result.Failure("Error deleting user address", ErrorCodeEnum.InternalError);
        }
    }
}