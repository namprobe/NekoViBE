using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Features.Dashboard.Queries.GetDashboardData;
using Swashbuckle.AspNetCore.Annotations;


namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/dashboard")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Dashboard")]
    [SwaggerTag("This API is used for Dashboard in CMS")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AuthorizeRoles]
        public async Task<IActionResult> GetDashboardData()
        {
            var result = await _mediator.Send(new GetDashboardDataQuery());
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
