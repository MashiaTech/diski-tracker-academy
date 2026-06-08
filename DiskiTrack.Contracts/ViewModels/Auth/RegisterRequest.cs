using System.ComponentModel.DataAnnotations;

namespace DiskiTrack.Contracts.ViewModels.Auth;

public sealed class RegisterRequest
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public string Role { get; set; } = "Coach";
}
