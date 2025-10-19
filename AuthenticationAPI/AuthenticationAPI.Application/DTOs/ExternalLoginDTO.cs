using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Application.DTOs
{
    public class ExternalLoginDTO
    {
        [Required]
        public string Provider { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
