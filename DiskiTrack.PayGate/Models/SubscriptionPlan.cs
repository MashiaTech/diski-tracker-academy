namespace DiskiTrack.PayGate.Models;

public sealed class SubscriptionPlan
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
}
