using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class PlayerMatchStats : AuditableEntity
{
    public Guid FixtureId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int MinutesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int ShotsTotal { get; set; }
    public int ShotsOnTarget { get; set; }
    public int PassesAttempted { get; set; }
    public int PassesCompleted { get; set; }
    public int KeyPasses { get; set; }
    public int Tackles { get; set; }
    public int TacklesWon { get; set; }
    public int Interceptions { get; set; }
    public int Recoveries { get; set; }
    public int Fouls { get; set; }
    public int FoulsWon { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int Saves { get; set; }
    public int ErrorsLeadingToGoal { get; set; }
    public decimal? CoachRating { get; set; } // 0.0–10.0

    // Navigation
    public Fixture Fixture { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
