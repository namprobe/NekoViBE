// Handler
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries;

public class UpdateAnimeSeriesCommandHandler
    : IRequestHandler<UpdateAnimeSeriesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAnimeSeriesCommandHandler> _logger;

    public UpdateAnimeSeriesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateAnimeSeriesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAnimeSeriesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var repo = _unitOfWork.Repository<Domain.Entities.AnimeSeries>();
            var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

            if (entity == null)
                return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);

            _mapper.Map(command.Request, entity);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                repo.Update(entity);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Anime series updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating anime series");
            return Result.Failure("Error updating anime series", ErrorCodeEnum.InternalError);
        }
    }
}
