using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetSelectList
{
    public class GetAnimeSeriesSelectListQueryHandler : IRequestHandler<GetAnimeSeriesSelectListQuery, List<AnimeSeriesSelectItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAnimeSeriesSelectListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AnimeSeriesSelectItem>> Handle(GetAnimeSeriesSelectListQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<Domain.Entities.AnimeSeries>().GetQueryable();

            if (!string.IsNullOrEmpty(request.Search))
                query = query.Where(x => x.Title.Contains(request.Search));

            return await query
                .OrderBy(x => x.Title)
                .Take(50)
                .Select(x => new AnimeSeriesSelectItem { Id = x.Id, Title = x.Title })
                .ToListAsync(cancellationToken);
        }
    }
}
