using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Queries.GetEvent
{
    public class GetEventQueryHandler : IRequestHandler<GetEventQuery, Result<EventResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventQueryHandler> _logger;

        public GetEventQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetEventQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<EventResponse>> Handle(GetEventQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .GetFirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted);
                if (entity == null)
                    return Result<EventResponse>.Failure("Event not found", ErrorCodeEnum.NotFound);

                var response = _mapper.Map<EventResponse>(entity);
                return Result<EventResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event with ID {Id}", query.Id);
                return Result<EventResponse>.Failure("Error getting event", ErrorCodeEnum.InternalError);
            }
        }
    }
}
