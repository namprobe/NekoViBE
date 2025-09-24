using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.Models;

public class BasePaginationFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    
    public string? SortBy { get; set; }
    
    public bool? IsAscending { get; set; }
    public EntityStatusEnum? Status { get; set; }
}