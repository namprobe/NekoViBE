using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserAddress.Commands.CreateUserAddress;

public class CreateUserAddressCommandHandler : IRequestHandler<CreateUserAddressCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserAddressCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceProvider _serviceProvider;
    
    public CreateUserAddressCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ILogger<CreateUserAddressCommandHandler> logger, 
        ICurrentUserService currentUserService, 
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Result> Handle(CreateUserAddressCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            
            var isCustomer = await _currentUserService.HasRoleAsync(RoleEnum.Customer);
            if (!isCustomer)
            {
                return Result.Failure("User is not a customer", ErrorCodeEnum.Forbidden);
            }

            var request = command.Request;
            var userAddress = _mapper.Map<Domain.Entities.UserAddress>(request);
            userAddress.UserId = userId.Value;
            userAddress.InitializeEntity(userId);
            
            await _unitOfWork.Repository<Domain.Entities.UserAddress>().AddAsync(userAddress);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Log user action using helper (fire and forget)
            UserActionHelper.LogCreateAction(
                _serviceProvider,
                userId.Value,
                userAddress.Id,
                userAddress,
                _currentUserService.IPAddress,
                cancellationToken
            );
            
            return Result.Success("User address created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user address");
            return Result.Failure("Error creating user address", ErrorCodeEnum.InternalError);
        }
    }
}