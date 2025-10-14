using ChallengeAPI.Domain.Entities;

public class ChallengeTask
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? TaskType { get; set; }
    public string? FrequencyRule { get; set; }
    public decimal? Points { get; set; }
    public bool IsRequired { get; set; }
    public string? ValidationRule { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Challenge Challenge { get; set; } = null!;
}
