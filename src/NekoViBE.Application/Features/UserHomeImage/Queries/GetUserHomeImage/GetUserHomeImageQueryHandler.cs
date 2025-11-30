// Handler
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImage
{
    public class GetUserHomeImageQueryHandler : IRequestHandler<GetUserHomeImageQuery, Result<UserHomeImageResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserHomeImageQueryHandler> _logger;

        public GetUserHomeImageQueryHandler(IUnitOfWork uow, IMapper mapper, ILogger<GetUserHomeImageQueryHandler> logger)
        {
            _unitOfWork = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<UserHomeImageResponse>> Handle(GetUserHomeImageQuery query, CancellationToken ct)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.UserHomeImage>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Id == query.Id && !x.IsDeleted,
                        includes: new Expression<Func<Domain.Entities.UserHomeImage, object>>[]
                        {
                            x => x.HomeImage!,
                            x => x.HomeImage!.AnimeSeries!
                        });

                if (entity == null)
                {
                    _logger.LogWarning("UserHomeImage {Id} not found", query.Id);
                    return Result<UserHomeImageResponse>.Failure("Not found", ErrorCodeEnum.NotFound);
                }

                var response = _mapper.Map<UserHomeImageResponse>(entity);
                return Result<UserHomeImageResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UserHomeImage {Id}", query.Id);
                return Result<UserHomeImageResponse>.Failure("Error", ErrorCodeEnum.InternalError);
            }
        }
    }
}