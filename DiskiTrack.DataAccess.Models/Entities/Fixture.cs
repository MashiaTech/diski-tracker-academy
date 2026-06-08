using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Fixture : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTime KickOffUtc { get; set; }
    public string Venue { get; set; } = string.Empty;
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Competition Competition { get; set; } = null!;
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public ICollection<FixtureEvent> Events { get; set; } = [];
    public ICollection<PlayerMatchStats> PlayerStats { get; set; } = [];
    public FixtureStats? Stats { get; set; }
    public ICollection<Lineup> Lineups { get; set; } = [];
    public ICollection<PlayerAvailability> Availabilities { get; set; } = [];
}
