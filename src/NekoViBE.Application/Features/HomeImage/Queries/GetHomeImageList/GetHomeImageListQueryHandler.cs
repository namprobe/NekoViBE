using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.HomeImage.Queries.GetHomeImageList
{
    public class GetHomeImageListQueryHandler : IRequestHandler<GetHomeImageListQuery, PaginationResult<HomeImageItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetHomeImageListQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetHomeImageListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetHomeImageListQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<PaginationResult<HomeImageItem>> Handle(GetHomeImageListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.HomeImage>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending,
                    includes: new[] { (Expression<Func<Domain.Entities.HomeImage, object>>)(x => x.AnimeSeries!) }
                );

                var dtos = _mapper.Map<List<HomeImageItem>>(items);

                foreach (var (dto, entity) in dtos.Zip(items, (d, e) => (d, e)))
                {
                    dto.ImagePath = _fileService.GetFileUrl(entity.ImagePath);
                }

                return PaginationResult<HomeImageItem>.Success(
                    dtos,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting home image list with filter: {@Filter}", request.Filter);
                return PaginationResult<HomeImageItem>.Failure("Error getting home image list", ErrorCodeEnum.InternalError);
            }
        }
    }
}