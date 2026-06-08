using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Competition : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid SeasonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CompetitionType Type { get; set; } = CompetitionType.League;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Season Season { get; set; } = null!;
    public ICollection<Fixture> Fixtures { get; set; } = [];
}
