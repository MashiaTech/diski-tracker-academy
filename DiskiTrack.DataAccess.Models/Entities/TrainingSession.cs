using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class TrainingSession : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid TeamId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public TrainingIntensity Intensity { get; set; } = TrainingIntensity.Medium;
    public string? Focus { get; set; }
    public string? Notes { get; set; }
    public string? CoachUserId { get; set; } // FK to ApplicationUser (string/Guid)

    // Navigation
    public Team Team { get; set; } = null!;
    public ICollection<TrainingAttendance> Attendance { get; set; } = [];
}
