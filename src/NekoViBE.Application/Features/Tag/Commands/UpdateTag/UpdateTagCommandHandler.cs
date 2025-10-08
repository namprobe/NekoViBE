using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.Tag.Commands.UpdateTag;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTagCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTagCommandHandler(IMapper mapper, IUnitOfWork unitOfWork,
        ILogger<UpdateTagCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi cập nhật Tag");
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
            {
                _logger.LogWarning("Người dùng không có quyền Admin để cập nhật Tag");
                return Result.Failure("User is not allowed to update Tag", ErrorCodeEnum.Forbidden);
            }

            var existingTag = await _unitOfWork.Repository<Domain.Entities.Tag>()
                .GetFirstOrDefaultAsync(x => x.Id == command.Id);

            if (existingTag == null)
            {
                return Result.Failure("Tag not found", ErrorCodeEnum.NotFound);
            }

            var oldValue = JsonSerializer.Serialize(_mapper.Map<TagRequest>(existingTag));
            var oldStatus = existingTag.Status;

            _mapper.Map(command.Request, existingTag);
            existingTag.UpdateEntity(userId.Value);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                _unitOfWork.Repository<Domain.Entities.Tag>().Update(existingTag);

                // Ghi log UserAction cho hành động cập nhật
                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Update,
                    EntityId = existingTag.Id,
                    EntityName = "Tag",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Cập nhật Tag với tên: {command.Request.Name}",
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
                        EntityId = existingTag.Id,
                        EntityName = "Tag",
                        OldValue = oldStatus.ToString(),
                        NewValue = command.Request.Status.ToString(),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Thay đổi trạng thái của Tag '{existingTag.Name}' từ {oldStatus} sang {command.Request.Status}",
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

            return Result.Success("Tag updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật Tag với ID: {TagId}", command.Id);
            return Result.Failure("Error updating Tag", ErrorCodeEnum.InternalError);
        }
    }
}