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

    public CreateAnimeSeriesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAnimeSeriesCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
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
                var fileName = $"{Guid.NewGuid()}_{command.Request.ImageFile.FileName}";
                var filePath = Path.Combine("wwwroot/images/anime-series", fileName); // Adjust storage path
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // Ensure directory exists
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.Request.ImageFile.CopyToAsync(stream, cancellationToken);
                }
                entity.ImagePath = $"/images/anime-series/{fileName}"; // Store relative path
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating anime series");
            return Result.Failure("Error creating anime series", ErrorCodeEnum.InternalError);
        }
    }
}