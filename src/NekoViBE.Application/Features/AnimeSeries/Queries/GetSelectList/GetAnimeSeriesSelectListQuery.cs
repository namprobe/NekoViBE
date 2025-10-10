using MediatR;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetSelectList
{
    public class GetAnimeSeriesSelectListQuery : IRequest<List<AnimeSeriesSelectItem>>
    {
        public string? Search { get; set; }
    }
}
