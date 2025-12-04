using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.CreateShippingMethod;

public class CreateShippingMethodHandler : IRequestHandler<CreateShippingMethodCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateShippingMethodHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateShippingMethodHandler(IMapper mapper, IUnitOfWork unitOfWork, 
        ILogger<CreateShippingMethodHandler> logger, ICurrentUserService currentUserService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CreateShippingMethodCommand command, CancellationToken cancellationToken)
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
                return Result.Failure("User is not allowed to create shipping method", ErrorCodeEnum.Forbidden);
            }
            
            var isNameExists = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .AnyAsync(x => x.Name == command.Request.Name);
            if (isNameExists)
            {
                return Result.Failure("Shipping method name already exists", ErrorCodeEnum.ValidationFailed);
            }
            
            var shippingMethod = _mapper.Map<Domain.Entities.ShippingMethod>(command.Request);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                shippingMethod.InitializeEntity(userId);
                await _unitOfWork.Repository<Domain.Entities.ShippingMethod>().AddAsync(shippingMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            
            return Result.Success("Shipping method created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping method");
            return Result.Failure("Error creating shipping method", ErrorCodeEnum.InternalError);
        }
    }
}
