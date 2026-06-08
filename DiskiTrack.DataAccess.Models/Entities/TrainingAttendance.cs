using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class TrainingAttendance : AuditableEntity
{
    public Guid TrainingSessionId { get; set; }
    public Guid PlayerId { get; set; }
    public bool Attended { get; set; }
    public int? Rpe { get; set; }           // Rate of Perceived Exertion 1–10
    public int? CoachRating { get; set; }   // 1–10
    public string? Notes { get; set; }

    // Navigation
    public TrainingSession TrainingSession { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
