using MediatR;
using NekoViBE.Application.Common.DTOs.Dashboard;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Dashboard.Queries.GetDashboardData
{
    public record GetDashboardDataQuery : IRequest<Result<DashboardDataDto>>;
}
