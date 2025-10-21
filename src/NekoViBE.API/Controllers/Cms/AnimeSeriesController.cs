using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Commands.DeleteAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeriesList;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller quản lý Anime Series cho CMS
/// </summary>
[ApiController]
[Route("api/cms/anime-series")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_AnimeSeries")]
[SwaggerTag("This API is used for Anime Series management in CMS")]
public class AnimeSeriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnimeSeriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all anime series with pagination and filtering",
        Description = "This API retrieves a paginated list of anime series with filtering options",
        OperationId = "GetAnimeSeriesList",
        Tags = new[] { "CMS", "CMS_AnimeSeries" }
    )]
    public async Task<IActionResult> GetAnimeSeriesList([FromQuery] AnimeSeriesFilter filter)
    {
        var query = new GetAnimeSeriesListQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    
    [HttpGet("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get anime series by ID",
        Description = "This API retrieves a specific anime series by its ID",
        OperationId = "GetAnimeSeries",
        Tags = new[] { "CMS", "CMS_AnimeSeries" }
    )]
    public async Task<IActionResult> GetAnimeSeries(Guid id)
    {
        var query = new GetAnimeSeriesQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    
    [HttpPost]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new anime series",
        Description = "This API creates a new anime series. It requires Admin role access",
        OperationId = "CreateAnimeSeries",
        Tags = new[] { "CMS", "CMS_AnimeSeries" }
    )]
    public async Task<IActionResult> CreateAnimeSeries([FromForm] AnimeSeriesRequest request)
    {
        var command = new CreateAnimeSeriesCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing anime series",
        Description = "This API updates an existing anime series. It requires Admin role access",
        OperationId = "UpdateAnimeSeries",
        Tags = new[] { "CMS", "CMS_AnimeSeries" }
    )]
    public async Task<IActionResult> UpdateAnimeSeries(Guid id, [FromForm] AnimeSeriesRequest request)
    {
        var command = new UpdateAnimeSeriesCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete an anime series",
        Description = "This API deletes an anime series. It requires Admin role access",
        OperationId = "DeleteAnimeSeries",
        Tags = new[] { "CMS", "CMS_AnimeSeries" }
    )]
    public async Task<IActionResult> DeleteAnimeSeries(Guid id)
    {
        var command = new DeleteAnimeSeriesCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpGet("select-list")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(List<AnimeSeriesSelectItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnimeSeriesSelectList([FromQuery] string? search)
    {
        var query = new GetAnimeSeriesSelectListQuery { Search = search };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

}