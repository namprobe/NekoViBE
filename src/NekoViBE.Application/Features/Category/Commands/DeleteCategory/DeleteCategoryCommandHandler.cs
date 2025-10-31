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
        private readonly IFileService _fileService;

        public DeleteCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteCategoryCommandHandler> logger,
            ICurrentUserService currentUserService, 
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
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

                var hasSubCategories = await repo.AnyAsync(x => x.ParentCategoryId == command.Id && !x.IsDeleted);

                var hasProducts = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.CategoryId == command.Id && !x.IsDeleted);
                if (hasSubCategories || hasProducts)
                    return Result.Failure("Cannot delete category with subcategories or products", ErrorCodeEnum.ResourceConflict);

                // Delete image if exists
                if (!string.IsNullOrEmpty(entity.ImagePath))
                {
                    await _fileService.DeleteFileAsync(entity.ImagePath, cancellationToken);
                }

                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Inactive;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    // Xử lý: cập nhật các Category con có ParentCategoryId = Id vừa xoá
                    var childCategories = await _unitOfWork.Repository<Domain.Entities.Category>()
                        .FindAsync(x => x.ParentCategoryId == entity.Id && !x.IsDeleted);

                    foreach (var child in childCategories)
                    {
                        child.ParentCategoryId = null;
                        child.UpdatedBy = userId;
                        child.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<Domain.Entities.Category>().Update(child);

                        _logger.LogInformation("Set ParentCategoryId = null for child category {ChildId} after deleting parent {ParentId}",
                            child.Id, entity.Id);
                    }

                    // Ghi lại UserAction
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "Category",
                        OldValue = JsonSerializer.Serialize(entity),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted category with name: {entity.Name}, updated {childCategories.Count()} children ParentCategoryId to null",
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
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting file for category with ID: {Id}", command.Id);
                return Result.Failure("Error deleting file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {Id}", command.Id);
                return Result.Failure("Error deleting category", ErrorCodeEnum.InternalError);
            }
        }

    }
}
