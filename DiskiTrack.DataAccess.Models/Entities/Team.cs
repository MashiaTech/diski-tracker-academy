using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Team : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AgeGroup { get; set; }
    public string? HomeVenue { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Player> Players { get; set; } = [];
}
