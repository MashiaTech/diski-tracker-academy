using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Models.Enums;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IGenericRepository<Player> _playerRepository;
    private readonly IGenericRepository<PlayerMatchStats> _playerStatsRepository;
    private readonly IGenericRepository<TrainingAttendance> _attendanceRepository;
    private readonly IGenericRepository<Fixture> _fixtureRepository;
    private readonly IGenericRepository<FixtureEvent> _eventRepository;

    public AnalyticsController(
        IGenericRepository<Player> playerRepository,
        IGenericRepository<PlayerMatchStats> playerStatsRepository,
        IGenericRepository<TrainingAttendance> attendanceRepository,
        IGenericRepository<Fixture> fixtureRepository,
        IGenericRepository<FixtureEvent> eventRepository)
    {
        _playerRepository = playerRepository;
        _playerStatsRepository = playerStatsRepository;
        _attendanceRepository = attendanceRepository;
        _fixtureRepository = fixtureRepository;
        _eventRepository = eventRepository;
    }

    [HttpGet("players/{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Parent)]
    public async Task<ActionResult> GetPlayerAnalytics(Guid id, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.QueryAsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.Id, p.FullName, p.TeamId, Position = p.PrimaryPosition.ToString() })
            .FirstOrDefaultAsync(cancellationToken);

        if (player is null)
            return NotFound("Player not found.");

        var recentStats = await _playerStatsRepository.QueryAsNoTracking()
            .Where(s => s.PlayerId == id)
            .OrderByDescending(s => s.Fixture.KickOffUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        var trainingWindowStart = DateTime.UtcNow.AddDays(-56);
        var attendanceRows = await _attendanceRepository.QueryAsNoTracking()
            .Where(a => a.PlayerId == id && a.TrainingSession.ScheduledAtUtc >= trainingWindowStart)
            .ToListAsync(cancellationToken);

        var matches = recentStats.Count;
        var minutes = recentStats.Sum(s => s.MinutesPlayed);
        var passesAttempted = recentStats.Sum(s => s.PassesAttempted);
        var passesCompleted = recentStats.Sum(s => s.PassesCompleted);
        var shotsOnTarget = recentStats.Sum(s => s.ShotsOnTarget);
        var tacklesWon = recentStats.Sum(s => s.TacklesWon);
        var recoveries = recentStats.Sum(s => s.Recoveries);
        var goals = recentStats.Sum(s => s.Goals);
        var assists = recentStats.Sum(s => s.Assists);

        var passCompletion = passesAttempted == 0 ? 0m : Math.Round((decimal)passesCompleted / passesAttempted * 100m, 1);
        var per90Factor = minutes == 0 ? 0m : 90m / minutes;

        var attendanceTotal = attendanceRows.Count;
        var attendancePresent = attendanceRows.Count(a => a.Attended);
        var attendanceRate = attendanceTotal == 0 ? 0m : Math.Round((decimal)attendancePresent / attendanceTotal * 100m, 1);

        var avgCoachRating = recentStats.Where(s => s.CoachRating.HasValue).Select(s => s.CoachRating!.Value).DefaultIfEmpty().Average();

        var trend = avgCoachRating switch
        {
            >= 7.5m => "Improving",
            <= 5.5m and > 0 => "Declining",
            _ => "Stable"
        };

        return Ok(new
        {
            Player = player,
            Trend = trend,
            Metrics = new
            {
                Matches = matches,
                MinutesPlayed = minutes,
                Goals = goals,
                Assists = assists,
                ShotsOnTarget = shotsOnTarget,
                TacklesWon = tacklesWon,
                Recoveries = recoveries,
                PassCompletionPercent = passCompletion,
                AttendancePercent = attendanceRate,
                GoalsPer90 = Math.Round(goals * per90Factor, 2),
                AssistsPer90 = Math.Round(assists * per90Factor, 2),
                AverageCoachRating = Math.Round(avgCoachRating, 2)
            }
        });
    }

    [HttpGet("teams/{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult> GetTeamAnalytics(Guid id, CancellationToken cancellationToken)
    {
        var fixtures = await _fixtureRepository.QueryAsNoTracking()
            .Where(f => f.HomeTeamId == id || f.AwayTeamId == id)
            .OrderByDescending(f => f.KickOffUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (fixtures.Count == 0)
            return Ok(new { TeamId = id, Message = "No fixture data available yet." });

        var fixtureIds = fixtures.Select(f => f.Id).ToArray();
        var teamStats = await _playerStatsRepository.QueryAsNoTracking()
            .Where(s => s.TeamId == id && fixtureIds.Contains(s.FixtureId))
            .ToListAsync(cancellationToken);

        var teamEvents = await _eventRepository.QueryAsNoTracking()
            .Where(e => e.TeamId == id && fixtureIds.Contains(e.FixtureId))
            .ToListAsync(cancellationToken);

        var goalsFor = fixtures.Sum(f => f.HomeTeamId == id ? (f.HomeScore ?? 0) : (f.AwayScore ?? 0));
        var goalsAgainst = fixtures.Sum(f => f.HomeTeamId == id ? (f.AwayScore ?? 0) : (f.HomeScore ?? 0));

        var passesAttempted = teamStats.Sum(s => s.PassesAttempted);
        var passesCompleted = teamStats.Sum(s => s.PassesCompleted);
        var possessionProxy = passesAttempted == 0 ? 0m : Math.Round((decimal)passesCompleted / passesAttempted * 100m, 1);

        var shots = teamStats.Sum(s => s.ShotsTotal);
        var shotsOnTarget = teamStats.Sum(s => s.ShotsOnTarget);
        var shotCreation = shots == 0 ? 0m : Math.Round((decimal)shotsOnTarget / shots * 100m, 1);

        var recoveries = teamStats.Sum(s => s.Recoveries);
        var interceptions = teamStats.Sum(s => s.Interceptions);
        var pressingProxy = recoveries + interceptions;
        var defensiveErrors = teamStats.Sum(s => s.ErrorsLeadingToGoal);

        var strengths = new List<string>();
        var weaknesses = new List<string>();

        if (possessionProxy >= 72) strengths.Add("Ball retention is a team strength.");
        else weaknesses.Add("Possession proxy is low; improve pass quality and support angles.");

        if (shotCreation >= 40) strengths.Add("Shot quality is trending positive.");
        else weaknesses.Add("Shot creation is weak; improve final third combinations.");

        if (pressingProxy >= fixtures.Count * 12) strengths.Add("Defensive recovery and pressing intensity are strong.");
        else weaknesses.Add("Pressing intensity proxy is low; improve counter-press timing.");

        if (defensiveErrors > fixtures.Count) weaknesses.Add("Defensive errors are costing chances/goals.");

        return Ok(new
        {
            TeamId = id,
            WindowMatches = fixtures.Count,
            Metrics = new
            {
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                GoalDifference = goalsFor - goalsAgainst,
                PossessionProxyPercent = possessionProxy,
                ShotCreationPercent = shotCreation,
                PressingIntensityProxy = pressingProxy,
                DefensiveErrors = defensiveErrors
            },
            Strengths = strengths,
            Weaknesses = weaknesses
        });
    }
}