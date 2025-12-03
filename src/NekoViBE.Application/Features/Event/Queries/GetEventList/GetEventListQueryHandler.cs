using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Queries.GetEventList
{
    public class GetEventListQueryHandler : IRequestHandler<GetEventListQuery, PaginationResult<EventItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public GetEventListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetEventListQueryHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<PaginationResult<EventItem>> Handle(GetEventListQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.Event>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                // Map sang EventItem
                var eventItems = _mapper.Map<List<EventItem>>(items);

                // Chuyển ImagePath thành URL đầy đủ
                foreach (var eventItem in eventItems)
                {
                    if (!string.IsNullOrEmpty(eventItem.ImagePath))
                    {
                        eventItem.ImagePath = _fileService.GetFileUrl(eventItem.ImagePath);
                    }
                }

                return PaginationResult<EventItem>.Success(eventItems, request.Filter.Page, request.Filter.PageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event list with filter: {@Filter}", request.Filter);
                return PaginationResult<EventItem>.Failure("Error getting event list", ErrorCodeEnum.InternalError);
            }
        }
    }
}
