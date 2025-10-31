using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Commands.DeletePayment;
public record DeletePaymentCommand(Guid Id) : IRequest<Result>;
