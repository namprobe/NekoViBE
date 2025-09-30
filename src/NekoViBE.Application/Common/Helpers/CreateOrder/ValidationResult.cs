using NekoViBE.Application.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Helpers.CreateOrder
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ErrorCodeEnum ErrorCode { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(string message, ErrorCodeEnum errorCode)
            => new() { IsValid = false, ErrorMessage = message, ErrorCode = errorCode };
    }
}
