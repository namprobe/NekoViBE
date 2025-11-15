// NekoViBE.Application.Features.PostCategory.Queries.GetPostCategoryList/GetPostCategoryListQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetPostCategoryList;

public record GetPostCategoryListQuery(PostCategoryFilter Filter) : IRequest<PaginationResult<PostCategoryItem>>;