// Handler
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImageList
{
    public class GetUserHomeImageListQueryHandler : IRequestHandler<GetUserHomeImageListQuery, PaginationResult<UserHomeImageItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserHomeImageListQueryHandler> _logger;

        public GetUserHomeImageListQueryHandler(IUnitOfWork uow, IMapper mapper, ILogger<GetUserHomeImageListQueryHandler> logger)
        {
            _unitOfWork = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginationResult<UserHomeImageItem>> Handle(GetUserHomeImageListQuery request, CancellationToken ct)
        {
            try
            {
                var predicate = request.Filter.BuildPredicate();

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: x => x.Position, // sắp xếp theo vị trí 1-2-3
                    isAscending: true,
                    includes: new Expression<Func<Domain.Entities.UserHomeImage, object>>[]
                    {
                        x => x.HomeImage!,
                        x => x.HomeImage!.AnimeSeries!
                    });

                var dtoItems = _mapper.Map<List<UserHomeImageItem>>(items);

                return PaginationResult<UserHomeImageItem>.Success(
                    dtoItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetUserHomeImageList");
                return PaginationResult<UserHomeImageItem>.Failure("Error retrieving list", ErrorCodeEnum.InternalError);
            }
        }
    }
}