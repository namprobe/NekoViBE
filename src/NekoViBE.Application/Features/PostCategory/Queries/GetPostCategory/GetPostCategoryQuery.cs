// NekoViBE.Application.Features.PostCategory.Queries.GetPostCategory/GetPostCategoryQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetPostCategory;

public record GetPostCategoryQuery(Guid Id) : IRequest<Result<PostCategoryResponse>>;