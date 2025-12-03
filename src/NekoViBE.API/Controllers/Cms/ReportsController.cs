using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Reports;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Report.Queries.GetDashboardSummary;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/reports")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Reports")]
    [SwaggerTag("This API is used for reports in CMS")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Get dashboard summary for CMS
        /// </summary>
        
        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<DashboardSummaryResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get dashboard summary", Tags = new[] { "CMS", "Reports" })]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var result = await _mediator.Send(new GetDashboardSummaryQuery());
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
