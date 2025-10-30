// NekoViBE.Application.Features.PostCategory.Commands.DeletePostCategory/DeletePostCategoryCommand.cs
using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Commands.DeletePostCategory;

public record DeletePostCategoryCommand(Guid Id) : IRequest<Result>;