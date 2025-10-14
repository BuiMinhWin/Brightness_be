using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BrightNess.Library.Jwt
{
    public interface ITokenService
    {
        string GenerateAccessToken(TokenPayload payload);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        AuthTokens GenerateAuthTokens(TokenPayload payload);
    }
}
