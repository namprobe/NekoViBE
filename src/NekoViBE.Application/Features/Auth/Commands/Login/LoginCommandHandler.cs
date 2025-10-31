using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService, ILogger<LoginCommandHandler> logger, IServiceProvider serviceProvider)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _identityService.AuthenticateAsync(command.Request);
            if (!result.IsSuccess)
            {
                return Result<AuthResponse>.Failure(result.Message?? "Invalid credentials", ErrorCodeEnum.InvalidCredentials);
            }

            var user = result.Data!;
            //generate refresh token and update auth infor of user
            var (refreshToken, refreshTokenExpiryTime) = _jwtService.GenerateRefreshTokenWithExpiration();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdateEntity(user.Id);
            await _identityService.UpdateUserAsync(user);
            //generate jwt token
            var (token, roles, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);
            var authResponse = new AuthResponse
            {
                AccessToken = token,
                Roles = roles,
                ExpiresAt = expiresAt
            };
            // Init shoppiong cart if not exists
            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var shoppingCart = await unitOfWork.Repository<ShoppingCart>().GetFirstOrDefaultAsync(x => x.UserId == user.Id);
                if (shoppingCart == null)
                {
                    shoppingCart = new ShoppingCart { UserId = user.Id };
                    shoppingCart.InitializeEntity(user.Id);
                    await unitOfWork.Repository<ShoppingCart>().AddAsync(shoppingCart);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            });
            return Result<AuthResponse>.Success(authResponse, "Login successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in");
            return Result<AuthResponse>.Failure("An error occurred while logging in", ErrorCodeEnum.InternalError);
        }
    }
}