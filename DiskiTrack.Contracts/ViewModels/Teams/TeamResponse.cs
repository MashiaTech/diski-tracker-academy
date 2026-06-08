namespace DiskiTrack.Contracts.ViewModels.Teams;

public sealed class TeamResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AgeGroup { get; set; }
    public string? HomeVenue { get; set; }
    public bool IsActive { get; set; }
    public int PlayerCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
