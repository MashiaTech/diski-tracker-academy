using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class LineupPlayer : BaseEntity
{
    public Guid LineupId { get; set; }
    public Guid PlayerId { get; set; }
    public Position Position { get; set; } = Position.Unknown;
    public bool IsStarting { get; set; } = true;
    public int? SubstitutedInMinute { get; set; }
    public int? SubstitutedOutMinute { get; set; }

    // Navigation
    public Lineup Lineup { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
