
using System;

namespace BrightNess.Library.Jwt
{
    public record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);
}
