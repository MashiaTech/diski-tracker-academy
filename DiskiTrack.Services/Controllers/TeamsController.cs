using DiskiTrack.Contracts.ViewModels.Teams;
using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Repository.Identity;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TeamsController : ControllerBase
{
    private readonly IGenericRepository<Team> _teamRepository;
    private readonly IGenericRepository<Tenant> _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public TeamsController(
        IGenericRepository<Team> teamRepository,
        IGenericRepository<Tenant> tenantRepository,
        ITenantContext tenantContext)
    {
        _teamRepository = teamRepository;
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<IReadOnlyList<TeamResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var teams = await _teamRepository.QueryAsNoTracking()
            .Select(t => new TeamResponse
            {
                Id = t.Id,
                TenantId = t.TenantId,
                Name = t.Name,
                AgeGroup = t.AgeGroup,
                HomeVenue = t.HomeVenue,
                IsActive = t.IsActive,
                PlayerCount = t.Players.Count,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(teams);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<TeamResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TeamResponse
            {
                Id = t.Id,
                TenantId = t.TenantId,
                Name = t.Name,
                AgeGroup = t.AgeGroup,
                HomeVenue = t.HomeVenue,
                IsActive = t.IsActive,
                PlayerCount = t.Players.Count,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<TeamResponse>> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var tenantId = await ResolveTenantId(cancellationToken);
        if (tenantId is null)
            return BadRequest("No tenant context found. Register/login first or create at least one tenant.");

        var team = new Team
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Name = request.Name,
            AgeGroup = request.AgeGroup,
            HomeVenue = request.HomeVenue,
            IsActive = true
        };

        await _teamRepository.AddAsync(team, cancellationToken);
        await _teamRepository.SaveChangesAsync(cancellationToken);

        var response = new TeamResponse
        {
            Id = team.Id,
            TenantId = team.TenantId,
            Name = team.Name,
            AgeGroup = team.AgeGroup,
            HomeVenue = team.HomeVenue,
            IsActive = team.IsActive,
            PlayerCount = 0,
            CreatedAtUtc = team.CreatedAtUtc
        };

        return CreatedAtAction(nameof(GetById), new { id = team.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<TeamResponse>> Update(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var team = await _teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
            return NotFound();

        team.Name = request.Name;
        team.AgeGroup = request.AgeGroup;
        team.HomeVenue = request.HomeVenue;
        team.IsActive = request.IsActive;

        _teamRepository.Update(team);
        await _teamRepository.SaveChangesAsync(cancellationToken);

        return Ok(new TeamResponse
        {
            Id = team.Id,
            TenantId = team.TenantId,
            Name = team.Name,
            AgeGroup = team.AgeGroup,
            HomeVenue = team.HomeVenue,
            IsActive = team.IsActive,
            PlayerCount = team.Players.Count,
            CreatedAtUtc = team.CreatedAtUtc
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdAsync(id, cancellationToken);
        if (team is null)
            return NotFound();

        _teamRepository.Remove(team);
        await _teamRepository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<Guid?> ResolveTenantId(CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId is not null)
            return _tenantContext.TenantId;

        var tenant = await _tenantRepository.QueryAsNoTracking().OrderBy(t => t.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        return tenant?.Id;
    }
}