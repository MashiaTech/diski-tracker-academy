using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class InjuryRecord : AuditableEntity
{
    public Guid PlayerId { get; set; }
    public DateOnly InjuryDate { get; set; }
    public DateOnly? EstimatedReturnDate { get; set; }
    public DateOnly? ActualReturnDate { get; set; }
    public string InjuryType { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public InjuryStatus Status { get; set; } = InjuryStatus.Active;
    public string? Notes { get; set; }

    // Navigation
    public Player Player { get; set; } = null!;
}
