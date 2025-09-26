using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;


namespace NekoViBE.Application.Features.Badge.Command.CreateBadge
{
    public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, Result>
    {
        private readonly ILogger<CreateBadgeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;


        public CreateBadgeCommandHandler(
            ILogger<CreateBadgeCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }
        public async Task<Result> Handle(CreateBadgeCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create badge");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var badgeRepo = _unitOfWork.Repository<Domain.Entities.Badge>();
                

                var entity = _mapper.Map<Domain.Entities.Badge>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                if (command.Request.IconPath != null)
                {
                    var imagePath = await _fileService.UploadFileAsync(command.Request.IconPath, "uploads/badge", cancellationToken);
                    entity.IconPath = imagePath;
                    _logger.LogInformation("ImagePath set to {ImagePath} for badge {Name}", imagePath, entity.Name);
                }
                else
                {
                    _logger.LogWarning("No ImageFile provided for badge {Name}", command.Request.Name);
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await badgeRepo.AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "Badge",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created badge with name: {command.Request.Name}",
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

                return Result.Success("Badge created successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file for badge");
                return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating badge");
                return Result.Failure("Error creating badge", ErrorCodeEnum.InternalError);
            }
        }
        //public async Task<Result> Handle(CreateBadgeCommand command, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
        //        if (!isValid)
        //        {

        //            _logger.LogWarning("Invalid or unauthenticated user attempting to create product");
        //            return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
        //        }

        //        var badge = _mapper.Map<Domain.Entities.Badge>(command.Request);
        //        badge.InitializeEntity(currentUserId);

        //        await _unitOfWork.Repository<Domain.Entities.Badge>().AddAsync(badge);
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //        return Result.Success("Badge created successfully");

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating product with name: {Name}", command.Request.Name);
        //        return Result.Failure("Error creating product", ErrorCodeEnum.InternalError);
        //    }
        //}
    }
}
