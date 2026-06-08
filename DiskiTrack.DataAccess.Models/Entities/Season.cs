using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Season : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Competition> Competitions { get; set; } = [];
}
