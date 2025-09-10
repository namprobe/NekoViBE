using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.PaymentMethod;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethods;

public record GetPaymentMethodsQuery(PaymentMethodFilter Filter) : IRequest<PaginationResult<PaymentMethodItem>>;