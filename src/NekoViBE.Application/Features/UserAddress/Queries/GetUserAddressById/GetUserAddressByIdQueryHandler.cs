using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserAddress.Queries.GetUserAddressById;

public class GetUserAddressByIdQueryHandler : IRequestHandler<GetUserAddressByIdQuery, Result<UserAddressDetail>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserAddressByIdQueryHandler> _logger;
    
    public GetUserAddressByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, ILogger<GetUserAddressByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService; 
        _logger = logger;
    }

    public async Task<Result<UserAddressDetail>> Handle(GetUserAddressByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
                return Result<UserAddressDetail>.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
            if (userAddress is null)
                return Result<UserAddressDetail>.Failure("User address not found", ErrorCodeEnum.NotFound);
            var userAddressDetail = _mapper.Map<UserAddressDetail>(userAddress);
            return Result<UserAddressDetail>.Success(userAddressDetail, "User address retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user address by id: {@Id}", request.Id);
            return Result<UserAddressDetail>.Failure("Error getting user address by id", ErrorCodeEnum.InternalError);
        }
    }
}