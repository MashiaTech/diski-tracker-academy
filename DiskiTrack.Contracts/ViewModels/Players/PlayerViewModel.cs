namespace DiskiTrack.Contracts.ViewModels.Players;

public sealed class PlayerViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}
