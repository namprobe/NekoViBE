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

namespace NekoViBE.Application.Features.Badge.Queries.GetBadge
{
    public class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, Result<BadgesResponse>>
    {
        private readonly ILogger<GetBadgesQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBadgesQueryHandler(ILogger<GetBadgesQueryHandler> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<BadgesResponse>> Handle(GetBadgesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var badges = await _unitOfWork.Repository<Domain.Entities.Badge>().GetAllAsync();
                var badgeDtos = _mapper.Map<List<BadgeDto>>(badges);

                var response = new BadgesResponse { Badges = badgeDtos };
                return Result<BadgesResponse>.Success(response, "Badges retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving badges");
                return Result<BadgesResponse>.Failure("Error retrieving badges", ErrorCodeEnum.InternalError);
            }
        }
    }

}
