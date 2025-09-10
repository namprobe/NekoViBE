using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Commands.DeleteAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeriesList;
using Microsoft.AspNetCore.Authorization;
using NekoViBE.Application.Common.Extensions;

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

    /// <summary>
    /// Get all anime series with pagination and filtering
    /// </summary>
    [HttpGet]
    [AuthorizeRoles("Admin", "Staff")]
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

    /// <summary>
    /// Get anime series by ID
    /// </summary>
    [HttpGet("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
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

    /// <summary>
    /// Create a new anime series
    /// </summary>
    [HttpPost]
    [AuthorizeRoles("Admin")]
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

    /// <summary>
    /// Update an existing anime series
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRoles("Admin")]
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

    /// <summary>
    /// Delete an anime series
    /// </summary>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin")]
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
}
