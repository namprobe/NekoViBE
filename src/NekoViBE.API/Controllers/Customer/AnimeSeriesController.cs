using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeries;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeriesList;
using NekoViBE.Application.Features.AnimeSeries.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/anime-series")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_AnimeSeries")]
    [SwaggerTag("This API is used for Anime Series management in customer")]
    public class AnimeSeriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnimeSeriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<AnimeSeriesItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get all anime series with pagination and filtering",
        Description = "This API retrieves a paginated list of anime series with filtering options",
        OperationId = "GetAnimeSeriesList",
        Tags = new[] { "Customer", "Customer_AnimeSeries" }
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
        [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<AnimeSeriesResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get anime series by ID",
            Description = "This API retrieves a specific anime series by its ID",
            OperationId = "GetAnimeSeries",
            Tags = new[] { "Customer", "Customer_AnimeSeries" }
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
}
