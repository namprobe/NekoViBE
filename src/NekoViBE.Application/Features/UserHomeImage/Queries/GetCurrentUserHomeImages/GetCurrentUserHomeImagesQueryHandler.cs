// File: Application/Features/UserHomeImage/Queries/GetCurrentUserHomeImages/GetCurrentUserHomeImagesQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetCurrentUserHomeImages
{
    public class GetCurrentUserHomeImagesQueryHandler : IRequestHandler<GetCurrentUserHomeImagesQuery, Result<List<UserHomeImageItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCurrentUserHomeImagesQueryHandler> _logger;

        public GetCurrentUserHomeImagesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<GetCurrentUserHomeImagesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<UserHomeImageItem>>> Handle(GetCurrentUserHomeImagesQuery request, CancellationToken ct)
        {
            try
            {
                // Lấy userId hiện tại
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || !userId.HasValue)
                {
                    return Result<List<UserHomeImageItem>>.Failure("User không hợp lệ hoặc chưa đăng nhập", ErrorCodeEnum.Unauthorized);
                }

                var entities = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
                    .FindAsync(
                        predicate: x => x.UserId == userId.Value && !x.IsDeleted,
                        includes: new Expression<Func<Domain.Entities.UserHomeImage, object>>[]
                        {
                            x => x.HomeImage!,
                            x => x.HomeImage!.AnimeSeries!
                        });

                var items = entities
                    .OrderBy(x => x.Position)
                    .Select(x => _mapper.Map<UserHomeImageItem>(x))
                    .ToList();

                return Result<List<UserHomeImageItem>>.Success(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting current user home images");
                return Result<List<UserHomeImageItem>>.Failure("Lỗi hệ thống", ErrorCodeEnum.InternalError);
            }
        }
    }
}