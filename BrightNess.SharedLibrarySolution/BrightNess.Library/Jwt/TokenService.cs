using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

        public string GenerateToken(TokenPayload payload)
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
    }
}
