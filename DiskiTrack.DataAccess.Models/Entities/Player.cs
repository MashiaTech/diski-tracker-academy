using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Player : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid TeamId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Position PrimaryPosition { get; set; } = Position.Unknown;
    public Position? SecondaryPosition { get; set; }
    public DominantFoot DominantFoot { get; set; } = DominantFoot.Unknown;
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public string? SchoolGrade { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianPhone { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Team Team { get; set; } = null!;
    public ICollection<PlayerMatchStats> MatchStats { get; set; } = [];
    public ICollection<InjuryRecord> Injuries { get; set; } = [];
    public ICollection<TrainingAttendance> TrainingAttendance { get; set; } = [];
    public ICollection<PlayerAvailability> Availabilities { get; set; } = [];
}
