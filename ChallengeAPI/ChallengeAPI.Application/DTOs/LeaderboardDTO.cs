
namespace ChallengeAPI.Application.DTOs
{
    public class LeaderboardDTO
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int Rank { get; set; }
        public decimal Score { get; set; }
    }
}
