using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;
using System.IO;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;

public class CreateAnimeSeriesCommandHandler : IRequestHandler<CreateAnimeSeriesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAnimeSeriesCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreateAnimeSeriesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAnimeSeriesCommandHandler> logger,
        ICurrentUserService currentUserService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<Result> Handle(CreateAnimeSeriesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                _logger.LogWarning("Invalid or unauthenticated user attempting to create anime series");
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            var entity = _mapper.Map<Domain.Entities.AnimeSeries>(command.Request);
            entity.CreatedBy = userId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = EntityStatusEnum.Active;

            // Handle image upload
            if (command.Request.ImageFile != null)
            {
                var imagePath = await _fileService.UploadFileAsync(command.Request.ImageFile, "uploads/anime-series", cancellationToken);
                entity.ImagePath = imagePath;
                _logger.LogInformation("ImagePath set to {ImagePath} for anime series {Title}", imagePath, entity.Title);
            }
            else
            {
                _logger.LogWarning("No ImageFile provided for anime series {Title}", command.Request.Title);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().AddAsync(entity);

                var userAction = new UserAction
                {
                    UserId = userId.Value,
                    Action = UserActionEnum.Create,
                    EntityId = entity.Id,
                    EntityName = "AnimeSeries",
                    NewValue = JsonSerializer.Serialize(command.Request),
                    IPAddress = _currentUserService.IPAddress ?? "Unknown",
                    ActionDetail = $"Created anime series with title: {command.Request.Title}",
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

            return Result.Success("Anime series created successfully");
        }
        catch (IOException ex)

        {

            _logger.LogError(ex, "Error uploading file for anime series");

            return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating anime series");
            return Result.Failure("Error creating anime series", ErrorCodeEnum.InternalError);
        }
    }
}