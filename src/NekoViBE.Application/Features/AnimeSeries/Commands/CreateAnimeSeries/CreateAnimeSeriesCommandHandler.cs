using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;

public class CreateAnimeSeriesCommandHandler
    : IRequestHandler<CreateAnimeSeriesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAnimeSeriesCommandHandler> _logger;

    public CreateAnimeSeriesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateAnimeSeriesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CreateAnimeSeriesCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var entity = _mapper.Map<Domain.Entities.AnimeSeries>(command.Request);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().AddAsync(entity);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Anime series created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating anime series");
            return Result.Failure("Error creating anime series", ErrorCodeEnum.InternalError);
        }
    }
}
