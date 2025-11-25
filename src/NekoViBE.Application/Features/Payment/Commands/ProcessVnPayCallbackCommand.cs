using MediatR;
using Microsoft.AspNetCore.Http;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Payment.Commands;

public record ProcessVnPayCallbackCommand(IQueryCollection QueryParams) : IRequest<Result<object>>;
