// NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory/UpdatePostCategoryCommand.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory;

public record UpdatePostCategoryCommand(Guid Id, PostCategoryRequest Request) : IRequest<Result>;