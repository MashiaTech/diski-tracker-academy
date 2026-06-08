using System.ComponentModel.DataAnnotations;

namespace DiskiTrack.Contracts.ViewModels.Teams;

public sealed class CreateTeamRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AgeGroup { get; set; }

    [MaxLength(200)]
    public string? HomeVenue { get; set; }
}
