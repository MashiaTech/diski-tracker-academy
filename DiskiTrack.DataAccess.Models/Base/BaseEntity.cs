namespace DiskiTrack.DataAccess.Models.Base;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
