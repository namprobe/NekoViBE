using MediatR;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingFee;

public record CalculateShippingFeeQuery(CalculateShippingFeeRequest Request) : IRequest<Result<ShippingFeeResult>>;

