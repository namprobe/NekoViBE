using MediatR;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(VerifyOtpRequest Request) : IRequest<Result>;
