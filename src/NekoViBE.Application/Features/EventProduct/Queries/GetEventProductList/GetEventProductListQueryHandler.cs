using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Queries.GetEventProductList
{
    public class GetEventProductListQueryHandler : IRequestHandler<GetEventProductListQuery, PaginationResult<EventProductItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventProductListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetEventProductListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetEventProductListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<EventProductItem>> Handle(GetEventProductListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, _) = await _currentUserService.IsUserValidAsync();
                if (!isValid)
                {
                    return PaginationResult<EventProductItem>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.EventProduct>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                var eventProductItems = _mapper.Map<List<EventProductItem>>(items);
                return PaginationResult<EventProductItem>.Success(eventProductItems, request.Filter.Page, request.Filter.PageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event product list with filter: {@Filter}", request.Filter);
                return PaginationResult<EventProductItem>.Failure("Error getting event product list", ErrorCodeEnum.InternalError);
            }
        }
    }
}
