// Handler
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.DeleteAnimeSeries;

public class DeleteAnimeSeriesCommandHandler
    : IRequestHandler<DeleteAnimeSeriesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAnimeSeriesCommandHandler> _logger;

    public DeleteAnimeSeriesCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteAnimeSeriesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAnimeSeriesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var repo = _unitOfWork.Repository<Domain.Entities.AnimeSeries>();
            var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id);

            if (entity == null)
                return Result.Failure("Anime series not found", ErrorCodeEnum.NotFound);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                repo.Delete(entity);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Result.Success("Anime series deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting anime series");
            return Result.Failure("Error deleting anime series", ErrorCodeEnum.InternalError);
        }
    }
}
