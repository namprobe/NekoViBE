using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.Tag.Commands.DeleteTag;

public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTagHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTagHandler(IUnitOfWork unitOfWork, ILogger<DeleteTagHandler> logger, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi xóa Tag");
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
            {
                _logger.LogWarning("Người dùng không có quyền Admin để xóa Tag");
                return Result.Failure("User is not allowed to delete Tag", ErrorCodeEnum.Forbidden);
            }
            var tag = await _unitOfWork.Repository<Domain.Entities.Tag>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (tag == null)
            {
                return Result.Failure("Tag not found", ErrorCodeEnum.NotFound);
            }

            // Kiểm tra xem Tag có đang được sử dụng không
            if (await _unitOfWork.Repository<Domain.Entities.ProductTag>().AnyAsync(x => x.TagId == request.Id) ||
                await _unitOfWork.Repository<Domain.Entities.PostTag>().AnyAsync(x => x.TagId == request.Id))
            {
                return Result.Failure("Tag is in use and cannot be deleted", ErrorCodeEnum.ValidationFailed);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                tag.IsDeleted = true;
                tag.DeletedBy = userId.Value;
                tag.DeletedAt = DateTime.UtcNow;
                tag.Status = EntityStatusEnum.Inactive;
                _unitOfWork.Repository<Domain.Entities.Tag>().Update(tag);

                // Ghi log UserAction cho hành động xóa
                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Delete,
                    EntityId = tag.Id,
                    EntityName = "Tag",
                    OldValue = JsonSerializer.Serialize(new { tag.Name, tag.Description, tag.Status }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Xóa Tag với tên: {tag.Name}",
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

            return Result.Success("Tag deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa Tag với ID: {TagId}", request.Id);
            return Result.Failure("Error deleting Tag", ErrorCodeEnum.InternalError);
        }
    }
}