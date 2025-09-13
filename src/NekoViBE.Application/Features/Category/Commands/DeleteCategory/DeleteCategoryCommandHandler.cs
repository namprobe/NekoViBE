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

namespace NekoViBE.Application.Features.Category.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteCategoryCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete category");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.Category>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Category not found", ErrorCodeEnum.NotFound);

                if (entity.SubCategories.Any() || entity.Products.Any())
                    return Result.Failure("Cannot delete category with subcategories or products", ErrorCodeEnum.ResourceConflict);

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "Category",
                        OldValue = JsonSerializer.Serialize(entity),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted category with name: {entity.Name}",
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

                return Result.Success("Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {Id}", command.Id);
                return Result.Failure("Error deleting category", ErrorCodeEnum.InternalError);
            }
        }
    }
}
