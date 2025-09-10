using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.UpdatePaymentMethod;

public record UpdatePaymentMethodCommand(Guid Id, PaymentMethodRequest Request) : IRequest<Result>;