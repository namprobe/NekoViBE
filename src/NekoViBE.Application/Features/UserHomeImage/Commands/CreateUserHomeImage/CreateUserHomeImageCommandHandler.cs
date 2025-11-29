using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.HomeImage.Commands.CreateHomeImage;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.UserHomeImage.Commands.CreateUserHomeImage
{
    public class CreateUserHomeImageCommandHandler : IRequestHandler<CreateUserHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateUserHomeImageCommandHandler> _logger;

        public CreateUserHomeImageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateUserHomeImageCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateUserHomeImageCommand command, CancellationToken ct)
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !userId.HasValue)
            {
                _logger.LogWarning("Unauthorized attempt to create UserHomeImage. User not authenticated.");
                return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
            }

            // Kiểm tra HomeImage có tồn tại không
            var homeImageExists = await _unitOfWork.Repository<Domain.Entities.HomeImage>()
                .AnyAsync(x => x.Id == command.Request.HomeImageId && !x.IsDeleted);
            if (!homeImageExists)
                return Result.Failure("Home image not found", ErrorCodeEnum.NotFound);

            // Giới hạn tối đa 3 ảnh
            var currentCount = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
                .CountAsync(x => x.UserId == userId.Value && !x.IsDeleted);
            if (currentCount >= 3)
                return Result.Failure("Maximum 3 home images allowed", ErrorCodeEnum.InvalidInput);

            // Position phải là 1,2,3 và không trùng
            if (command.Request.Position < 1 || command.Request.Position > 3)
                return Result.Failure("Position must be 1, 2 or 3", ErrorCodeEnum.InvalidInput);

            var positionTaken = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
                .AnyAsync(x => x.UserId == userId.Value && x.Position == command.Request.Position && !x.IsDeleted);
            if (positionTaken)
                return Result.Failure($"Position {command.Request.Position} is already taken", ErrorCodeEnum.InvalidInput);

            var entity = _mapper.Map<Domain.Entities.UserHomeImage>(command.Request);
            entity.UserId = userId.Value;
            entity.CreatedBy = userId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync(ct);
            await _unitOfWork.Repository<Domain.Entities.UserHomeImage>().AddAsync(entity);

            var userAction = new UserAction
            {
                UserId = userId.Value,
                Action = UserActionEnum.Create,
                EntityId = entity.Id,
                EntityName = nameof(UserHomeImage),                  
                NewValue = JsonSerializer.Serialize(new
                {
                    entity.UserId,
                    entity.HomeImageId,
                    entity.Position
                }),
                OldValue = null,                                   
                IPAddress = _currentUserService.IPAddress ?? "Unknown",
                ActionDetail = $"User added home image (HomeImageId: {command.Request.HomeImageId}) to position {command.Request.Position}",
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatusEnum.Active
            };

            await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

            await _unitOfWork.CommitTransactionAsync(ct);

            return Result.Success("Added to your home screen successfully");
        }
    }
}
