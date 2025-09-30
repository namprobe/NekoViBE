using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;
    private readonly IFileServiceFactory _fileServiceFactory;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
    ILogger<UpdateProfileCommandHandler> logger, IFileServiceFactory fileServiceFactory, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _fileServiceFactory = fileServiceFactory;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (valid, userId) = await _currentUserService.IsUserValidAsync();
            if (!valid || userId == null)
                return Result.Failure("Invalid user.", ErrorCodeEnum.Unauthorized);
            var user = await _unitOfWork.Repository<AppUser>().GetFirstOrDefaultAsync(u => u.Id == userId, c => c.CustomerProfile!);
            if (user == null)
                return Result.Failure("User not found.", ErrorCodeEnum.NotFound);
            var profile = user.CustomerProfile;
            _mapper.Map(request.UpdateProfileRequest, user);
            _mapper.Map(request.UpdateProfileRequest, profile);
            _logger.LogInformation("request bio and profile bio after mapping: {Bio} - {ProfileBio}", request.UpdateProfileRequest.Bio, profile?.Bio);
            user.UpdateEntity(userId.Value);
            profile?.UpdateEntity(userId.Value);
            string? oldAvatarPath = user.AvatarPath;
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                if (request.UpdateProfileRequest.Avatar != null)
                {
                    var fileService = _fileServiceFactory.CreateFileService("local");
                    var avatarPath = await fileService.UploadFileAsync(request.UpdateProfileRequest.Avatar, "uploads/avatars", cancellationToken);
                    user.AvatarPath = avatarPath;
                }
                _unitOfWork.Repository<AppUser>().Update(user);
                if (profile != null)
                    _unitOfWork.Repository<CustomerProfile>().Update(profile);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Delete old avatar file if a new one was uploaded fire-and-forget
                if (oldAvatarPath != null && request.UpdateProfileRequest.Avatar != null)
                    _ = Task.Run((
                        async () =>
                        {
                        try {
                            var fileService = _fileServiceFactory.CreateFileService("local");
                            await fileService.DeleteFileAsync(oldAvatarPath, cancellationToken);
                        } catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deleting old avatar file at {OldAvatarPath}", oldAvatarPath);
                        }
                    }));
                return Result.Success("Profile updated successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user");
                return Result.Failure("An error occurred while updating the profile.", ErrorCodeEnum.InternalError);
            }

    }
}