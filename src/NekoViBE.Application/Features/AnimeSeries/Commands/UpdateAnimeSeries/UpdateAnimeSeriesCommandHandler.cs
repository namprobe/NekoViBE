using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries
{
    public class UpdateAnimeSeriesCommandHandler : IRequestHandler<UpdateAnimeSeriesCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateAnimeSeriesCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateAnimeSeriesCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateAnimeSeriesCommandHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateAnimeSeriesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update anime series");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.AnimeSeries>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

                if (entity == null)
                    return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);

                var oldValue = JsonSerializer.Serialize(_mapper.Map<AnimeSeriesRequest>(entity));
                var oldStatus = entity.Status;
                var oldImagePath = entity.ImagePath;

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                if (command.Request.ImageFile != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        await _fileService.DeleteFileAsync(oldImagePath, cancellationToken);
                    }

                    // Upload new image
                    var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads", cancellationToken);
                    entity.ImagePath = imagePath;
                    _logger.LogInformation("ImagePath updated to {ImagePath} for anime series {Title}", imagePath, entity.Title);
                }
                else
                {
                    // If no new image provided, keep the old image or set to null if explicitly cleared
                    _logger.LogInformation("No ImageFile provided for anime series {Title}", entity.Title);
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
                        EntityName = "AnimeSeries",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated anime series with title: {command.Request.Title}",
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
                            EntityName = "AnimeSeries",
                            OldValue = oldStatus.ToString(),
                            NewValue = command.Request.Status.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of anime series '{entity.Title}' from {oldStatus} to {command.Request.Status}",
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

                return Result.Success("Anime series updated successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error handling file for anime series");
                return Result.Failure("Error handling file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating anime series with ID: {Id}", command.Id);
                return Result.Failure("Error updating anime series", ErrorCodeEnum.InternalError);
            }
        }
    }
}