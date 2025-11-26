using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Auth.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileResponse>>
{
    private readonly ILogger<GetProfileQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetProfileQueryHandler(ILogger<GetProfileQueryHandler> logger, IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || userId == null)
            {
                return Result<ProfileResponse>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            //Determine profile type based on roles (includes expression params)
            Expression<Func<AppUser, object>>? include = null; //admin will be null
            include = roles.Contains(RoleEnum.Customer) ? u => u.CustomerProfile! : u => u.StaffProfile!;

            var user = await _unitOfWork.Repository<AppUser>().GetFirstOrDefaultAsync(
                u => u.Id == userId, include);
            var response = _mapper.Map<ProfileResponse>(user);
            response.Bio = user?.CustomerProfile?.Bio?? user?.StaffProfile?.Bio;
            // AvatarPath is already converted to full URL by AvatarPathUrlResolver
            return Result<ProfileResponse>.Success(response, "Profile retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile for user {UserId}", _currentUserService.UserId);
            return Result<ProfileResponse>.Failure("An error occurred while retrieving profile", ErrorCodeEnum.InternalError);
        }
    }
}