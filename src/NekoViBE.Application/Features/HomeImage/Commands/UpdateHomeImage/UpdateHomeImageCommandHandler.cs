// UpdateHomeImageCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.HomeImage.Commands.UpdateHomeImage
{
    public class UpdateHomeImageCommandHandler : IRequestHandler<UpdateHomeImageCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateHomeImageCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public UpdateHomeImageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<UpdateHomeImageCommandHandler> logger, ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<Result> Handle(UpdateHomeImageCommand command, CancellationToken ct)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);

                var repo = _unitOfWork.Repository<Domain.Entities.HomeImage>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);
                if (entity == null)
                    return Result.Failure("Home image not found", ErrorCodeEnum.NotFound);

                if (command.Request.AnimeSeriesId.HasValue &&
                    !await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().AnyAsync(x => x.Id == command.Request.AnimeSeriesId.Value))
                    return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);

                var oldImagePath = entity.ImagePath;
                var oldValue = JsonSerializer.Serialize(new { entity.ImagePath, entity.AnimeSeriesId });

                // Nếu có file mới → upload + xóa cũ
                if (command.Request.ImageFile != null)
                {
                    var newPath = await _fileService.UploadFileAsync(command.Request.ImageFile, "home-images", ct);
                    entity.ImagePath = newPath;

                    if (!string.IsNullOrEmpty(oldImagePath))
                        await _fileService.DeleteFileAsync(oldImagePath, ct);
                }

                entity.AnimeSeriesId = command.Request.AnimeSeriesId;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.BeginTransactionAsync(ct);
                repo.Update(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Update,
                    EntityId = entity.Id,
                    EntityName = "HomeImage",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(new { entity.ImagePath, entity.AnimeSeriesId }),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = "Updated home image",
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };
                await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                await _unitOfWork.CommitTransactionAsync(ct);
                return Result.Success("Home image updated successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error handling file for home image {Id}", command.Id);
                return Result.Failure("Error handling image file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating home image {Id}", command.Id);
                return Result.Failure("Error updating home image", ErrorCodeEnum.InternalError);
            }
        }
    }
}