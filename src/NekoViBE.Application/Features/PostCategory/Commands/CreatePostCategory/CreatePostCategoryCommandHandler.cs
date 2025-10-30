// CreatePostCategoryCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.PostCategory.Commands.CreatePostCategory
{
    // CreatePostCategoryCommandHandler.cs
    public class CreatePostCategoryCommandHandler : IRequestHandler<CreatePostCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePostCategoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreatePostCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<CreatePostCategoryCommandHandler> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreatePostCategoryCommand command, CancellationToken ct)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);

                var entity = _mapper.Map<Domain.Entities.PostCategory>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                await _unitOfWork.Repository<Domain.Entities.PostCategory>().AddAsync(entity);

                var action = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = entity.Id,
                    EntityName = "PostCategory",
                    NewValue = System.Text.Json.JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Created category: {entity.Name}",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(action);

                await _unitOfWork.CommitTransactionAsync(ct);
                return Result.Success("Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post category");
                return Result.Failure("Error creating category", ErrorCodeEnum.InternalError);
            }
        }
    }
}