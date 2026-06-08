using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DiskiTrack.Contracts.ViewModels.Auth;
using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Repository.Identity;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IGenericRepository<Tenant> _tenantRepository;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IGenericRepository<Tenant> tenantRepository,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tenantRepository = tenantRepository;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var requestedRole = string.IsNullOrWhiteSpace(request.Role) ? "Coach" : request.Role.Trim();
        var isSystemAdmin = requestedRole.Equals("SystemAdmin", StringComparison.OrdinalIgnoreCase);

        Guid tenantId;
        if (isSystemAdmin)
        {
            tenantId = Guid.Empty;
        }
        else
        {
            if (request.TenantId is null)
                return BadRequest("TenantId is required for non-SystemAdmin users.");

            var tenantExists = await _tenantRepository.AnyAsync(t => t.Id == request.TenantId.Value, cancellationToken);
            if (!tenantExists)
                return BadRequest("Tenant does not exist.");

            tenantId = request.TenantId.Value;
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Conflict("A user with this email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            UserName = request.Email,
            Email = request.Email,
            TenantId = tenantId,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        if (!string.IsNullOrWhiteSpace(requestedRole))
        {
            var roleExists = await _roleManager.RoleExistsAsync(requestedRole);
            if (!roleExists)
            {
                var createRole = await _roleManager.CreateAsync(new IdentityRole<Guid>(requestedRole));
                if (!createRole.Succeeded)
                    return BadRequest(createRole.Errors.Select(e => e.Description));
            }

            var userAlreadyInRole = await _userManager.IsInRoleAsync(user, requestedRole);
            if (!userAlreadyInRole)
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, requestedRole);
                if (!addRoleResult.Succeeded)
                    return BadRequest(addRoleResult.Errors.Select(e => e.Description));
            }
        }

        return Ok(await BuildTokenResponseAsync(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Unauthorized("Invalid credentials.");

        return Ok(await BuildTokenResponseAsync(user));
    }

    private async Task<AuthResponse> BuildTokenResponseAsync(ApplicationUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "DiskiTrack";
        var audience = jwtSection["Audience"] ?? "DiskiTrack";
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var expiresMinutes = int.TryParse(jwtSection["ExpiresInMinutes"], out var minutes) ? minutes : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (user.TenantId != Guid.Empty)
            claims.Add(new Claim("tenant_id", user.TenantId.ToString()));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt
        };
    }
}