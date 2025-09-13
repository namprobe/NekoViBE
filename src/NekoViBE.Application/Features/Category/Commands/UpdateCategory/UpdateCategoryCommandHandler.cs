using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Category;
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

namespace NekoViBE.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateCategoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update category");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Category>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Category not found", ErrorCodeEnum.NotFound);

                var oldValue = JsonSerializer.Serialize(_mapper.Map<CategoryRequest>(entity));
                var oldStatus = entity.Status;
                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                if (command.Request.ImageFile != null)
                {
                    _logger.LogInformation("Received ImageFile: {FileName}, Size: {FileSize}",
                        command.Request.ImageFile.FileName, command.Request.ImageFile.Length);
                    var fileName = $"{Guid.NewGuid()}_{command.Request.ImageFile.FileName}";
                    var filePath = Path.Combine("wwwroot/images/categories", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await command.Request.ImageFile.CopyToAsync(stream, cancellationToken);
                    }
                    entity.ImagePath = $"/images/categories/{fileName}";
                    _logger.LogInformation("ImagePath updated to {ImagePath} for category {Name}",
                        entity.ImagePath, entity.Name);
                }
                else
                {
                    _logger.LogInformation("No ImageFile provided for category {Name}, setting ImagePath to null", entity.Name);
                    entity.ImagePath = null;
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "Category",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated category with name: {command.Request.Name}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    if (oldStatus != command.Request.Status)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = entity.Id,
                            EntityName = "Category",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of category '{entity.Name}' from {oldStatus} to {command.Request.Status}",
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

                return Result.Success("Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return Result.Failure("Error updating category", ErrorCodeEnum.InternalError);
            }
        }
    }
}
