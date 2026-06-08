using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class FixtureEvent : AuditableEntity
{
    public Guid FixtureId { get; set; }
    public Guid? PlayerId { get; set; }
    public Guid? SecondaryPlayerId { get; set; }  // e.g. assist provider
    public Guid TeamId { get; set; }
    public EventType Type { get; set; }
    public int Minute { get; set; }
    public CardType? CardType { get; set; }
    public bool IsHomeTeam { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Fixture Fixture { get; set; } = null!;
    public Player? Player { get; set; }
    public Player? SecondaryPlayer { get; set; }
    public Team Team { get; set; } = null!;
}
