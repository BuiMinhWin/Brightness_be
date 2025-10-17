public record ChallengeTaskDTO(
    Guid Id,
    Guid ChallengeId,
    string Name,
    string? Description,
    string? TaskType,
    string? FrequencyRule,
    decimal? Points,
    bool IsRequired,
    string? ValidationRule,
    DateTime? StartDate,
    DateTime? EndDate
);
