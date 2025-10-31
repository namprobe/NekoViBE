using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Queries.GetTag
{
    public class GetTagQueryHandler : IRequestHandler<GetTagQuery, Result<TagResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetTagQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagQueryHandler> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<TagResponse>> Handle(GetTagQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid)
                {
                    return Result<TagResponse>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }
                var tag = await _unitOfWork.Repository<Domain.Entities.Tag>().GetFirstOrDefaultAsync(x => x.Id == request.Id);
                if (tag == null)
                {
                    return Result<TagResponse>.Failure("Tag not found", ErrorCodeEnum.NotFound);
                }
                var tagResponse = _mapper.Map<TagResponse>(tag);
                return Result<TagResponse>.Success(tagResponse, "Tag retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Tag");
                return Result<TagResponse>.Failure("Error retrieving Tag", ErrorCodeEnum.InternalError);
            }
        }
    }
}
