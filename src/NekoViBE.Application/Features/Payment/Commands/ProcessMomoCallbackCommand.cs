using MediatR;
using NekoViBE.Application.Common.Models;
using PaymentService.Application.Commons.Models.Momo;

namespace NekoViBE.Application.Features.Payment.Commands;

public record ProcessMomoCallbackCommand(MoMoIpnRequest Request, string CallerIpAddress) : IRequest<Result<MoMoIpnResponse>>;

