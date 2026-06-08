namespace DiskiTrack.PayGate.Models;

public sealed class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
}
