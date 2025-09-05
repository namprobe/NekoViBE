using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Common;
using System.Transactions;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(ILogger<RegisterCommandHandler> logger, IIdentityService identityService, 
    IMapper mapper, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _identityService = identityService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = _mapper.Map<AppUser>(command.Request);
            user.Id = Guid.NewGuid();
            var customerProfile = _mapper.Map<CustomerProfile>(command.Request);
            user.InitializeEnitity(user.Id);
            customerProfile.UserId = user.Id;
            customerProfile.InitializeEnitity(customerProfile.Id);
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(1)
                },
                TransactionScopeAsyncFlowOption.Enabled
            ))
            {
                var createResult = await _identityService.CreateUserAsync(user, command.Request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
                }
                
                var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Customer.ToString());
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to add user to role", ErrorCodeEnum.ValidationFailed, errors);
                }
                
                await _unitOfWork.Repository<CustomerProfile>().AddAsync(customerProfile);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                scope.Complete();
            }

            // create shopping cart for customer (no need to wait for the task to complete)
            _ = Task.Run(async () =>
            {
                var newShoppingCart = new ShoppingCart{
                    UserId = user.Id,
                };
                newShoppingCart.InitializeEnitity(user.Id);
                await _unitOfWork.Repository<ShoppingCart>().AddAsync(newShoppingCart);
                await _unitOfWork.SaveChangesAsync();
            });

            if (command.Request.Avatar != null)
            {
                // upload avatar to storage fire and foreground processing (future feature)
                _logger.LogInformation("Has file upload, but not implemented yet");
            }
            return Result.Success("Register successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering");
            return Result.Failure("Error registering", ErrorCodeEnum.InternalError);
        }
    }
}