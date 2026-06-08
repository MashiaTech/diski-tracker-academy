namespace DiskiTrack.PayGate.Models;

public sealed class Invoice
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
