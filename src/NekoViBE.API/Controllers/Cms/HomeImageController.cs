using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.HomeImage.Commands.CreateHomeImage;
using NekoViBE.Application.Features.HomeImage.Commands.DeleteHomeImage;
using NekoViBE.Application.Features.HomeImage.Commands.UpdateHomeImage;
using NekoViBE.Application.Features.HomeImage.Queries.GetHomeImage;
using NekoViBE.Application.Features.HomeImage.Queries.GetHomeImageList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/home-images")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_HomeImage")]
    [SwaggerTag("This API is used for Home Image management in CMS")]
    public class HomeImageController : ControllerBase
    {
        private readonly IMediator _mediator;
        public HomeImageController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AuthorizeRoles]
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

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Create new home image", OperationId = "CreateHomeImage")]
        public async Task<IActionResult> Create([FromForm] HomeImageRequest request)
        {
            var result = await _mediator.Send(new CreateHomeImageCommand(request));
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        // HomeImageController.cs
        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update home image", OperationId = "UpdateHomeImage")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateHomeImageDTO request) // ← ĐỔI TẠI ĐÂY
        {
            var result = await _mediator.Send(new UpdateHomeImageCommand(id, request));
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Delete home image", OperationId = "DeleteHomeImage")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteHomeImageCommand(id));
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}