using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Application.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
