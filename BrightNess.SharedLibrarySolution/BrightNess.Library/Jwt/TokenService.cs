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
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateAccessToken(TokenPayload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            if (string.IsNullOrWhiteSpace(_options.Key))
                throw new InvalidOperationException("JWT signing key is missing or empty.");

            var keyBytes = Encoding.UTF8.GetBytes(_options.Key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: payload.ToClaims(),
                expires: DateTime.UtcNow.AddMinutes(Math.Max(1, _options.ExpirationMinutes)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        public AuthTokens GenerateAuthTokens(TokenPayload payload)
        {
            var accessToken = GenerateAccessToken(payload);
            var refreshToken = GenerateRefreshToken();
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.ExpirationMinutes));
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays);

            return new AuthTokens(accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt);
        }
    }
}
