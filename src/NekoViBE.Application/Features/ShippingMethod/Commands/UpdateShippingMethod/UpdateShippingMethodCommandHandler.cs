using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.UpdateShippingMethod;

public class UpdateShippingMethodCommandHandler : IRequestHandler<UpdateShippingMethodCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateShippingMethodCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UpdateShippingMethodCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, 
        ILogger<UpdateShippingMethodCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateShippingMethodCommand command, CancellationToken cancellationToken)
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
                return Result.Failure("User is not allowed to update shipping method", ErrorCodeEnum.Forbidden);
            }

            var existingShippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == command.Id);
            
            if (existingShippingMethod == null)
            {
                return Result.Failure("Shipping method not found", ErrorCodeEnum.NotFound);
            }
            
            var isNameExists = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .AnyAsync(x => x.Name == command.Request.Name && x.Id != command.Id);
            if (isNameExists)
            {
                return Result.Failure("Shipping method name already exists", ErrorCodeEnum.ValidationFailed);
            }
            
            _mapper.Map(command.Request, existingShippingMethod);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                existingShippingMethod.UpdateEntity(userId);
                
                _unitOfWork.Repository<Domain.Entities.ShippingMethod>().Update(existingShippingMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Shipping method updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipping method with ID: {ShippingMethodId}", command.Id);
            return Result.Failure("Error updating shipping method", ErrorCodeEnum.InternalError);
        }
    }
}
