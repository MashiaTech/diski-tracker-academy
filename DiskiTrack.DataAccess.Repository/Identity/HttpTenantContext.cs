using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DiskiTrack.DataAccess.Repository.Identity;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue("tenant_id");

            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
