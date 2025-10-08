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

namespace NekoViBE.Application.Features.Tag.Commands.CreateTag;

public class CreateTagHandler : IRequestHandler<CreateTagCommand, Result>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTagHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateTagHandler(IMapper mapper, IUnitOfWork unitOfWork,
        ILogger<CreateTagHandler> logger, ICurrentUserService currentUserService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi tạo Tag");
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
            {
                _logger.LogWarning("Người dùng không có quyền Admin để tạo Tag");
                return Result.Failure("User is not allowed to create Tag", ErrorCodeEnum.Forbidden);
            }
            var tag = _mapper.Map<Domain.Entities.Tag>(command.Request);
            tag.InitializeEntity(userId.Value);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork.Repository<Domain.Entities.Tag>().AddAsync(tag);

                // Ghi log UserAction cho hành động tạo
                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = tag.Id,
                    EntityName = "Tag",
                    NewValue = JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Tạo Tag với tên: {command.Request.Name}",
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

            return Result.Success("Tag created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Tag");
            return Result.Failure("Error creating Tag", ErrorCodeEnum.InternalError);
        }
    }
}