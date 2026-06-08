namespace DiskiTrack.PushNotification.Models;

public sealed class NotificationMessage
{
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
