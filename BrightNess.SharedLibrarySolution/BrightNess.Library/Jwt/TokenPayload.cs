using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrightNess.Library.Jwt
{
    public class TokenPayload
    {
        public string? Id { get; set; } // Thêm thuộc tính Id
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public IDictionary<string, string>? ExtraClaims { get; set; }

        public IEnumerable<Claim> ToClaims()
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(Id)) claims.Add(new Claim(ClaimTypes.NameIdentifier, Id!)); // Thêm Id vào claims
            if (!string.IsNullOrWhiteSpace(Name)) claims.Add(new Claim(ClaimTypes.Name, Name!));
            if (!string.IsNullOrWhiteSpace(Email)) claims.Add(new Claim(ClaimTypes.Email, Email!));
            if (!string.IsNullOrWhiteSpace(Role)) claims.Add(new Claim(ClaimTypes.Role, Role!));
            if (ExtraClaims != null)
            {
                foreach (var kv in ExtraClaims)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null)
                        claims.Add(new Claim(kv.Key, kv.Value));
                }
            }
            return claims;
        }
    }
}
