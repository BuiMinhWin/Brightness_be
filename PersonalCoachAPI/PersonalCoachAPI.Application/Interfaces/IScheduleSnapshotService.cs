using PersonalCoachAPI.Application.DTOs;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IScheduleSnapshotService
    {
        Task<Guid> CreateSnapshotAsync(CreateSnapshotRequest request);

        Task<List<ScheduleSnapshotResponse>> GetScheduleSnapshotsAsync(Guid scheduleId, int limit = 100);

        Task<List<ScheduleSnapshotResponse>> GetUserSnapshotsAsync(Guid userId, int limit = 100);

        Task<UserScheduleAnalytics> GetUserAnalyticsAsync(Guid userId);

        Task<WorkoutScheduleResponse> RevertToSnapshotAsync(Guid snapshotId, Guid userId);

        Task<ScheduleSnapshotResponse?> GetSnapshotAsync(Guid snapshotId);
    }
}

