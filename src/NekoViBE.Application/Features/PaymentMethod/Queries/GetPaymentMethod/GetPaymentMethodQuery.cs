using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethod;

public record GetPaymentMethodQuery(Guid Id) : IRequest<Result<PaymentMethodResponse>>;