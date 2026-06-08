namespace DiskiTrack.Services.Application.Interfaces;

public interface IAuthService
{
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
