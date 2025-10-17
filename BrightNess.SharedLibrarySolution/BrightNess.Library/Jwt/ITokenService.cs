using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BrightNess.Library.Jwt
{
    public interface ITokenService
    {
        string GenerateAccessToken(TokenPayload payload);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        AuthTokens GenerateAuthTokens(TokenPayload payload, int? customRefreshTokenDays = null);
        string? ExtractJtiFromToken(string token);
        DateTime? GetTokenExpiry(string token);
    }
}
