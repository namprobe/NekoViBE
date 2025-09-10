using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.CreatePaymentMethod;

public record CreatePaymentMethodCommand(PaymentMethodRequest Request) : IRequest<Result>;