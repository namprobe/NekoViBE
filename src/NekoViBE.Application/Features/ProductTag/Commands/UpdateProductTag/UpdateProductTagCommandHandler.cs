using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.ProductTag.Commands.UpdateProductTag
{
    public class UpdateProductTagCommandHandler : IRequestHandler<UpdateProductTagCommand, Result>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProductTagCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductTagCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ILogger<UpdateProductTagCommandHandler> logger, ICurrentUserService currentUserService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateProductTagCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi cập nhật ProductTag");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
                {
                    _logger.LogWarning("Người dùng không có quyền Admin để cập nhật ProductTag");
                    return Result.Failure("User is not allowed to update ProductTag", ErrorCodeEnum.Forbidden);
                }

                var existingProductTag = await _unitOfWork.Repository<Domain.Entities.ProductTag>()
                    .GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (existingProductTag == null)
                {
                    return Result.Failure("ProductTag not found", ErrorCodeEnum.NotFound);
                }

                // Kiểm tra sự tồn tại của Product và Tag
                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.Id == command.Request.ProductId);
                if (!productExists)
                {
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                var tagExists = await _unitOfWork.Repository<Domain.Entities.Tag>().AnyAsync(x => x.Id == command.Request.TagId);
                if (!tagExists)
                {
                    return Result.Failure("Tag not found", ErrorCodeEnum.NotFound);
                }

                var oldValue = JsonSerializer.Serialize(_mapper.Map<ProductTagRequest>(existingProductTag));
                var oldStatus = existingProductTag.Status;

                _mapper.Map(command.Request, existingProductTag);
                existingProductTag.UpdateEntity(userId.Value);

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    _unitOfWork.Repository<Domain.Entities.ProductTag>().Update(existingProductTag);

                    // Ghi log UserAction cho hành động cập nhật
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = existingProductTag.Id,
                        EntityName = "ProductTag",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Cập nhật ProductTag với ProductId: {command.Request.ProductId}, TagId: {command.Request.TagId}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    // Ghi log UserAction cho thay đổi trạng thái (nếu có)
                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = existingProductTag.Id,
                            EntityName = "ProductTag",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Thay đổi trạng thái của ProductTag từ {oldStatus} sang {command.Request.Status}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("ProductTag updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ProductTag với ID: {ProductTagId}", command.Id);
                return Result.Failure("Error updating ProductTag", ErrorCodeEnum.InternalError);
            }
        }
    }
}
