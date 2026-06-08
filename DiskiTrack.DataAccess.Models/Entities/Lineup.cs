using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Lineup : AuditableEntity
{
    public Guid FixtureId { get; set; }
    public Guid TeamId { get; set; }
    public string Formation { get; set; } = string.Empty; // e.g. "4-3-3"

    // Navigation
    public Fixture Fixture { get; set; } = null!;
    public Team Team { get; set; } = null!;
    public ICollection<LineupPlayer> Players { get; set; } = [];
}
