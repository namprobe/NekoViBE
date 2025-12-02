using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.UpdatePaymentMethod;

public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePaymentMethodCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileServiceFactory _fileServiceFactory;

    public UpdatePaymentMethodCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, 
        ILogger<UpdatePaymentMethodCommandHandler> logger, ICurrentUserService currentUserService,
        IFileServiceFactory fileServiceFactory)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
        _fileServiceFactory = fileServiceFactory;
    }

    public async Task<Result> Handle(UpdatePaymentMethodCommand command, CancellationToken cancellationToken)
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
                return Result.Failure("User is not allowed to update payment method", ErrorCodeEnum.Forbidden);
            }

            // Check if payment method exists
            var existingPaymentMethod = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == command.Id);
            
            if (existingPaymentMethod == null)
            {
                return Result.Failure("Payment method not found", ErrorCodeEnum.NotFound);
            }
            var isNameExists = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().AnyAsync(x => x.Name == command.Request.Name.ToString() && x.Id != command.Id);
            if (isNameExists)
            {
                return Result.Failure("Payment method name already exists", ErrorCodeEnum.ValidationFailed);
            }
            
            // Store old icon path for deletion if new image is uploaded
            string? oldIconPath = existingPaymentMethod.IconPath;
            
            // Map updated data to existing entity
            _mapper.Map(command.Request, existingPaymentMethod);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                // Upload new icon image if provided
                if (command.Request.IconImage != null)
                {
                    var fileService = _fileServiceFactory.CreateFileService("local");
                    var iconPath = await fileService.UploadFileAsync(command.Request.IconImage, "uploads/payment-methods", cancellationToken);
                    existingPaymentMethod.IconPath = iconPath;
                }
                
                // Update audit fields
                existingPaymentMethod.UpdateEntity(userId);
                
                _unitOfWork.Repository<Domain.Entities.PaymentMethod>().Update(existingPaymentMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Delete old icon file if a new one was uploaded (fire-and-forget)
                if (oldIconPath != null && command.Request.IconImage != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var fileService = _fileServiceFactory.CreateFileService("local");
                            await fileService.DeleteFileAsync(oldIconPath, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deleting old payment method icon file at {OldIconPath}", oldIconPath);
                        }
                    });
                }
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Payment method updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method with ID: {PaymentMethodId}", command.Id);
            return Result.Failure("Error updating payment method", ErrorCodeEnum.InternalError);
        }
    }
}
