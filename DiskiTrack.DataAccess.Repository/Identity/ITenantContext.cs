namespace DiskiTrack.DataAccess.Repository.Identity;

public interface ITenantContext
{
    Guid? TenantId { get; }
}
