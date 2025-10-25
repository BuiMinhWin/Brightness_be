using System;

namespace ChatService.Application.Exceptions
{
    public class InsufficientCreditsException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public InsufficientCreditsException(string message, string errorCode = "INSUFFICIENT_CREDITS", int statusCode = 402)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}

