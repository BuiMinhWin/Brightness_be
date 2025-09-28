using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightNess.Library.Jwt
{
    public interface ITokenService
    {
        string GenerateToken(TokenPayload payload);
    }
}
