using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.DeleteAnimeSeries
{
    public class DeleteAnimeSeriesCommandHandler : IRequestHandler<DeleteAnimeSeriesCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteAnimeSeriesCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public DeleteAnimeSeriesCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteAnimeSeriesCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(DeleteAnimeSeriesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to delete anime series");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.AnimeSeries>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);

                // Check for dependencies
                //var hasProducts = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.AnimeSeriesId == command.Id);
                //if (hasProducts)
                //    return Result.Failure("Cannot delete anime series with associated products", ErrorCodeEnum.ResourceConflict);

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

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Delete,
                        EntityId = entity.Id,
                        EntityName = "AnimeSeries",
                        OldValue = JsonSerializer.Serialize(new { entity.Title, entity.ReleaseYear, entity.Description }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Deleted anime series with title: {entity.Title}",
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

                return Result.Success("Anime series deleted successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting file for anime series with ID: {Id}", command.Id);
                return Result.Failure("Error deleting file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting anime series with ID: {Id}", command.Id);
                return Result.Failure("Error deleting anime series", ErrorCodeEnum.InternalError);
            }
        }
    }
}