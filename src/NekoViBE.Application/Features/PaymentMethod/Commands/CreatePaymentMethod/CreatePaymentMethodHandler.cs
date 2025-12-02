using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.CreatePaymentMethod;

public class CreatePaymentMethodHandler : IRequestHandler<CreatePaymentMethodCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePaymentMethodHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileServiceFactory _fileServiceFactory;

    public CreatePaymentMethodHandler(IMapper mapper, IUnitOfWork unitOfWork, 
    ILogger<CreatePaymentMethodHandler> logger, ICurrentUserService currentUserService,
    IFileServiceFactory fileServiceFactory)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
        _fileServiceFactory = fileServiceFactory;
    }

    public async Task<Result> Handle(CreatePaymentMethodCommand command, CancellationToken cancellationToken)
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
                return Result.Failure("User is not allowed to create payment method", ErrorCodeEnum.Forbidden);
            }
            var isNameExists = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().AnyAsync(x => x.Name == command.Request.Name.ToString());
            if (isNameExists)
            {
                return Result.Failure("Payment method name already exists", ErrorCodeEnum.ValidationFailed);
            }
            var paymentMethod = _mapper.Map<Domain.Entities.PaymentMethod>(command.Request);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                // Upload icon image if provided
                if (command.Request.IconImage != null)
                {
                    var fileService = _fileServiceFactory.CreateFileService("local");
                    var iconPath = await fileService.UploadFileAsync(command.Request.IconImage, "uploads/payment-methods", cancellationToken);
                    paymentMethod.IconPath = iconPath;
                }
                
                paymentMethod.InitializeEntity(userId);
                await _unitOfWork.Repository<Domain.Entities.PaymentMethod>().AddAsync(paymentMethod);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            
            return Result.Success("Payment method created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method");
            return Result.Failure("Error creating payment method", ErrorCodeEnum.InternalError);
        }
    }
}