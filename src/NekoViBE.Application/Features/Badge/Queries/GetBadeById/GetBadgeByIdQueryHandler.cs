using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Badge.Queries.GetBadeById
{
    public class GetBadgeByIdQueryHandler : IRequestHandler<GetBadgeByIdQuery, Result<BadgeDto>>
    {
        private readonly ILogger<GetBadgeByIdQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBadgeByIdQueryHandler(ILogger<GetBadgeByIdQueryHandler> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<BadgeDto>> Handle(GetBadgeByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var badge = await _unitOfWork.Repository<Domain.Entities.Badge>().GetByIdAsync(request.Id);
                if (badge == null)
                {
                    return Result<BadgeDto>.Failure("Badge not found", ErrorCodeEnum.NotFound);
                }

                var badgeDto = _mapper.Map<BadgeDto>(badge);
                return Result<BadgeDto>.Success(badgeDto, "Badge retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving badge {BadgeId}", request.Id);
                return Result<BadgeDto>.Failure("Error retrieving badge", ErrorCodeEnum.InternalError);
            }
        }
    }
}
