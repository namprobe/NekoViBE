// GetHomeImageQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.HomeImage.Queries.GetHomeImage
{
    public class GetHomeImageQueryHandler : IRequestHandler<GetHomeImageQuery, Result<HomeImageResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetHomeImageQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetHomeImageQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<GetHomeImageQueryHandler> logger, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<HomeImageResponse>> Handle(GetHomeImageQuery query, CancellationToken ct)
        {
            var entity = await _unitOfWork.Repository<Domain.Entities.HomeImage>()
                .GetFirstOrDefaultAsync(x => x.Id == query.Id, x => x.AnimeSeries);

            if (entity == null)
                return Result<HomeImageResponse>.Failure("Home image not found", ErrorCodeEnum.NotFound);

            var response = _mapper.Map<HomeImageResponse>(entity);
            response.ImagePath = _fileService.GetFileUrl(entity.ImagePath);

            return Result<HomeImageResponse>.Success(response);
        }
    }
}