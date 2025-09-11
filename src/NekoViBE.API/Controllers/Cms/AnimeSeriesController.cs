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
    /// <remarks>
    /// This API retrieves a paginated list of anime series with filtering options.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/anime-series?page=1&amp;pageSize=10&amp;title=Naruto&amp;releaseYear=2002
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">Anime series filter parameters</param>
    /// <returns>Paginated list of anime series</returns>
    /// <response code="200">Anime series retrieved successfully</response>
    /// <response code="401">Failed to retrieve anime series (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve anime series (internal server error)</response>
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
    /// <remarks>
    /// This API retrieves a specific anime series by its ID.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/anime-series/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Anime series ID</param>
    /// <returns>Anime series details</returns>
    /// <response code="200">Anime series retrieved successfully</response>
    /// <response code="401">Failed to retrieve anime series (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="404">Anime series not found</response>
    /// <response code="500">Failed to retrieve anime series (internal server error)</response>
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
    /// <remarks>
    /// This API creates a new anime series. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/anime-series
    ///     Content-Type: multipart/form-data
    ///     
    ///     {
    ///        "title": "One Piece",
    ///        "description": "A pirate adventure",
    ///        "releaseYear": 1999,
    ///        "status": 1,
    ///        "imageFile": [file]
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="request">Anime series creation request</param>
    /// <returns>Creation result</returns>
    /// <response code="200">Anime series created successfully</response>
    /// <response code="400">Creation failed (validation error)</response>
    /// <response code="401">Failed to create anime series (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="500">Failed to create anime series (internal server error)</response>
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
    /// <remarks>
    /// This API updates an existing anime series. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/anime-series/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: multipart/form-data
    ///     
    ///     {
    ///        "title": "One Piece Updated",
    ///        "description": "Updated pirate adventure",
    ///        "releaseYear": 1999,
    ///        "status": 1,
    ///        "imageFile": [file]
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Anime series ID</param>
    /// <param name="request">Anime series update request</param>
    /// <returns>Update result</returns>
    /// <response code="200">Anime series updated successfully</response>
    /// <response code="400">Update failed (validation error)</response>
    /// <response code="401">Failed to update anime series (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Anime series not found</response>
    /// <response code="500">Failed to update anime series (internal server error)</response>
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
    /// <remarks>
    /// This API deletes an anime series. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/cms/anime-series/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Anime series ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Anime series deleted successfully</response>
    /// <response code="401">Failed to delete anime series (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Anime series not found</response>
    /// <response code="409">Anime series is in use and cannot be deleted</response>
    /// <response code="500">Failed to delete anime series (internal server error)</response>
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