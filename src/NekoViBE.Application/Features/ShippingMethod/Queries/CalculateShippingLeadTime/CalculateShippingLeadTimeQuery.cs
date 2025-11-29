using MediatR;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingLeadTime;

public record CalculateShippingLeadTimeQuery(CalculateShippingFeeRequest Request) : IRequest<Result<ShippingLeadTimeResult>>;

