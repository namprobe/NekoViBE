using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.HomeImage.Queries.GetHomeImage;
using NekoViBE.Application.Features.HomeImage.Queries.GetHomeImageList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/home-images")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_HomeImage")]
    [SwaggerTag("This API is used for Home Image management in Customer")]
    public class HomeImageController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HomeImageController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<HomeImageItem>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get all home images with pagination and filtering", OperationId = "GetHomeImageList")]
        public async Task<IActionResult> GetList([FromQuery] HomeImageFilter filter)
        {
            var result = await _mediator.Send(new GetHomeImageListQuery(filter));
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<HomeImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<HomeImageResponse>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get home image by ID", OperationId = "GetHomeImage")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetHomeImageQuery(id));
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
