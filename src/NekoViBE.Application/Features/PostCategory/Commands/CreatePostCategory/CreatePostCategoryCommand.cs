// CreatePostCategoryCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Commands.CreatePostCategory
{
    public record CreatePostCategoryCommand(PostCategoryRequest Request) : IRequest<Result>;
}