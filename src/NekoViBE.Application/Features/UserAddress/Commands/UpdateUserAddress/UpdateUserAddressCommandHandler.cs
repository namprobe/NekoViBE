using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserAddress.Commands.UpdateUserAddress;

public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateUserAddressCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UpdateUserAddressCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService, 
        ILogger<UpdateUserAddressCommandHandler> logger, 
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Result> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
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
            
            if (userAddress.UserId != userId)
            {
                return Result.Failure("You are not authorized to update this user address", ErrorCodeEnum.Forbidden);
            }

            // Clone old address for logging
            var oldUserAddressJson = JsonSerializer.Serialize(userAddress);
            var oldUserAddress = JsonSerializer.Deserialize<Domain.Entities.UserAddress>(oldUserAddressJson);
            
            _mapper.Map(request.Request, userAddress);
            userAddress.UpdateEntity(userId);
            
            _unitOfWork.Repository<Domain.Entities.UserAddress>().Update(userAddress);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Log user action using helper (fire and forget)
            UserActionHelper.LogUpdateAction(
                _serviceProvider,
                userId.Value,
                userAddress.Id,
                oldUserAddress!,
                userAddress,
                _currentUserService.IPAddress,
                cancellationToken
            );
            
            return Result.Success("User address updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user address: {@Request}", request);
            return Result.Failure("Error updating user address", ErrorCodeEnum.InternalError);
        }
    }
}
