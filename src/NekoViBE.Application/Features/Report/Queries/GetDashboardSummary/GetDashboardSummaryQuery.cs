using MediatR;
using NekoViBE.Application.Common.DTOs.Reports;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Report.Queries.GetDashboardSummary
{
    public record GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryResponse>>;
}
