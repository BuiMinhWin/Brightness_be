using System;

namespace AuthenticationAPI.Application.DTOs
{
    public record AuthTokensDTO(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);
}
