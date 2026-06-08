using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Models.Enums;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/opponents")]
public sealed class OpponentsController : ControllerBase
{
    private readonly IGenericRepository<Team> _teamRepository;
    private readonly IGenericRepository<Fixture> _fixtureRepository;
    private readonly IGenericRepository<PlayerMatchStats> _playerStatsRepository;

    public OpponentsController(
        IGenericRepository<Team> teamRepository,
        IGenericRepository<Fixture> fixtureRepository,
        IGenericRepository<PlayerMatchStats> playerStatsRepository)
    {
        _teamRepository = teamRepository;
        _fixtureRepository = fixtureRepository;
        _playerStatsRepository = playerStatsRepository;
    }

    [HttpGet("{id:guid}/profile")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        var opponent = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Id, t.Name, t.AgeGroup, t.HomeVenue })
            .FirstOrDefaultAsync(cancellationToken);

        if (opponent is null)
            return NotFound("Opponent not found.");

        var recentFixtures = await _fixtureRepository.QueryAsNoTracking()
            .Where(f => (f.HomeTeamId == id || f.AwayTeamId == id) && f.Status == MatchStatus.Finished)
            .OrderByDescending(f => f.KickOffUtc)
            .Take(8)
            .ToListAsync(cancellationToken);

        var fixtureIds = recentFixtures.Select(f => f.Id).ToArray();
        var stats = await _playerStatsRepository.QueryAsNoTracking()
            .Where(s => s.TeamId == id && fixtureIds.Contains(s.FixtureId))
            .ToListAsync(cancellationToken);

        var goalsFor = recentFixtures.Sum(f => f.HomeTeamId == id ? (f.HomeScore ?? 0) : (f.AwayScore ?? 0));
        var goalsAgainst = recentFixtures.Sum(f => f.HomeTeamId == id ? (f.AwayScore ?? 0) : (f.HomeScore ?? 0));

        var passesAttempted = stats.Sum(s => s.PassesAttempted);
        var passesCompleted = stats.Sum(s => s.PassesCompleted);
        var passRetention = passesAttempted == 0 ? 0m : Math.Round((decimal)passesCompleted / passesAttempted * 100m, 1);
        var recoveries = stats.Sum(s => s.Recoveries);
        var defensiveErrors = stats.Sum(s => s.ErrorsLeadingToGoal);

        var tendencies = new List<string>();
        if (goalsAgainst >= recentFixtures.Count)
            tendencies.Add("Concedes frequently; test them with sustained pressure.");
        if (passRetention < 68)
            tendencies.Add("Build-up under pressure is vulnerable.");
        if (recoveries > recentFixtures.Count * 14)
            tendencies.Add("Strong ball recovery profile; avoid risky central turnovers.");

        if (tendencies.Count == 0)
            tendencies.Add("Balanced profile with no obvious extreme trend in recent data.");

        return Ok(new
        {
            Opponent = opponent,
            RecentWindow = recentFixtures.Count,
            Metrics = new
            {
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                PassRetentionPercent = passRetention,
                Recoveries = recoveries,
                DefensiveErrors = defensiveErrors
            },
            TacticalTendencies = tendencies
        });
    }
}