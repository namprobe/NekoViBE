using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;

namespace NekoViBE.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(IIdentityService identityService, ILogger<LogoutCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Result.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            }
            var result = await _identityService.GetUserByIdAsync(userId);
            if (!result.IsSuccess)
            {
                return Result.Failure(result.Message?? "User not found", ErrorCodeEnum.NotFound);
            }
            var user = result.Data!;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdateEntity(Guid.Parse(userId));
            await _identityService.UpdateUserAsync(user);
            return Result.Success("Logout successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out");
            return Result.Failure("An error occurred while logging out", ErrorCodeEnum.InternalError);
        }
    }
}