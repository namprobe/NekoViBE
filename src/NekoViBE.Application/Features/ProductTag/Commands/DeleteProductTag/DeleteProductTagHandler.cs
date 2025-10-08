using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Commands.DeleteProductTag
{
    public class DeleteProductTagHandler : IRequestHandler<DeleteProductTagCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteProductTagHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductTagHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductTagHandler> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteProductTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi xóa ProductTag");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }
                if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
                {
                    _logger.LogWarning("Người dùng không có quyền Admin để xóa ProductTag");
                    return Result.Failure("User is not allowed to delete ProductTag", ErrorCodeEnum.Forbidden);
                }
                var productTag = await _unitOfWork.Repository<Domain.Entities.ProductTag>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
                if (productTag == null)
                {
                    return Result.Failure("ProductTag not found", ErrorCodeEnum.NotFound);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    productTag.IsDeleted = true;
                    productTag.DeletedBy = userId.Value;
                    productTag.DeletedAt = DateTime.UtcNow;
                    productTag.Status = EntityStatusEnum.Inactive;
                    _unitOfWork.Repository<Domain.Entities.ProductTag>().Update(productTag);

                    // Ghi log UserAction cho hành động xóa
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = productTag.Id,
                        EntityName = "ProductTag",
                        OldValue = JsonSerializer.Serialize(new { productTag.ProductId, productTag.TagId, productTag.Status }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Xóa ProductTag với ProductId: {productTag.ProductId}, TagId: {productTag.TagId}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("ProductTag deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa ProductTag với ID: {ProductTagId}", request.Id);
                return Result.Failure("Error deleting ProductTag", ErrorCodeEnum.InternalError);
            }
        }
    }
}
