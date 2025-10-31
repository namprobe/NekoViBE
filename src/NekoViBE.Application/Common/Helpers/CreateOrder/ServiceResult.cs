using NekoViBE.Application.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Helpers.CreateOrder
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public ErrorCodeEnum ErrorCode { get; set; }

        public static ServiceResult<T> Success(T data, string message = "")
            => new() { IsSuccess = true, Data = data, Message = message };

        public static ServiceResult<T> Failure(string message, ErrorCodeEnum errorCode)
            => new() { IsSuccess = false, Message = message, ErrorCode = errorCode };
    }
}
