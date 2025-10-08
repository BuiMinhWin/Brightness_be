using System;

namespace BrightNess.Library.Responses
{
    public record Response(bool Flag = false, string Message = null!, object? Data = null);
}
