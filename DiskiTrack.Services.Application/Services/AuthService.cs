using DiskiTrack.Services.Application.Interfaces;

namespace DiskiTrack.Services.Application.Services;

public sealed class AuthService : IAuthService
{
    public Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password));
}
