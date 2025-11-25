// NekoViBE.Application.Features.PostCategory.Queries.GetSelectList/GetPostCategorySelectListQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.PostCategory;
using System.Collections.Generic;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetSelectList;

public class GetPostCategorySelectListQuery : IRequest<List<PostCategorySelectItem>>
{
    public string? Search { get; set; }
}