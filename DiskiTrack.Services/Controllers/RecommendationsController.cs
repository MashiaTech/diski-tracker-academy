using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Models.Enums;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly IGenericRepository<Fixture> _fixtureRepository;
    private readonly IGenericRepository<Player> _playerRepository;
    private readonly IGenericRepository<PlayerAvailability> _availabilityRepository;
    private readonly IGenericRepository<PlayerMatchStats> _playerStatsRepository;
    private readonly IGenericRepository<TrainingAttendance> _attendanceRepository;
    private readonly IGenericRepository<Prediction> _predictionRepository;

    public RecommendationsController(
        IGenericRepository<Fixture> fixtureRepository,
        IGenericRepository<Player> playerRepository,
        IGenericRepository<PlayerAvailability> availabilityRepository,
        IGenericRepository<PlayerMatchStats> playerStatsRepository,
        IGenericRepository<TrainingAttendance> attendanceRepository,
        IGenericRepository<Prediction> predictionRepository)
    {
        _fixtureRepository = fixtureRepository;
        _playerRepository = playerRepository;
        _availabilityRepository = availabilityRepository;
        _playerStatsRepository = playerStatsRepository;
        _attendanceRepository = attendanceRepository;
        _predictionRepository = predictionRepository;
    }

    [HttpGet("fixture/{fixtureId:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst)]
    public async Task<ActionResult> RecommendForFixture(Guid fixtureId, [FromQuery] Guid? teamId, CancellationToken cancellationToken)
    {
        var fixture = await _fixtureRepository.GetByIdAsync(fixtureId, cancellationToken);
        if (fixture is null)
            return NotFound("Fixture not found.");

        var targetTeamId = teamId ?? fixture.HomeTeamId;
        if (targetTeamId != fixture.HomeTeamId && targetTeamId != fixture.AwayTeamId)
            return BadRequest("teamId must be one of the fixture teams.");

        var opponentTeamId = targetTeamId == fixture.HomeTeamId ? fixture.AwayTeamId : fixture.HomeTeamId;

        var players = await _playerRepository.QueryAsNoTracking()
            .Where(p => p.TeamId == targetTeamId && p.IsActive)
            .ToListAsync(cancellationToken);

        if (players.Count == 0)
            return BadRequest("No active players found for the selected team.");

        var unavailable = await _availabilityRepository.QueryAsNoTracking()
            .Where(a => a.FixtureId == fixtureId && a.Status != AvailabilityStatus.Available)
            .Select(a => a.PlayerId)
            .ToListAsync(cancellationToken);

        var candidatePlayers = players.Where(p => !unavailable.Contains(p.Id)).ToList();
        if (candidatePlayers.Count < 11)
            return BadRequest("Not enough available players to build a starting XI.");

        var candidateIds = candidatePlayers.Select(p => p.Id).ToArray();
        var stats = await _playerStatsRepository.QueryAsNoTracking()
            .Where(s => s.TeamId == targetTeamId && candidateIds.Contains(s.PlayerId))
            .OrderByDescending(s => s.Fixture.KickOffUtc)
            .ToListAsync(cancellationToken);

        var attendanceSince = DateTime.UtcNow.AddDays(-42);
        var attendance = await _attendanceRepository.QueryAsNoTracking()
            .Where(a => candidateIds.Contains(a.PlayerId) && a.TrainingSession.ScheduledAtUtc >= attendanceSince)
            .ToListAsync(cancellationToken);

        var scored = candidatePlayers.Select(player =>
        {
            var recent = stats.Where(s => s.PlayerId == player.Id).Take(5).ToList();
            var attendanceRows = attendance.Where(a => a.PlayerId == player.Id).ToList();

            var avgRating = recent.Where(r => r.CoachRating.HasValue).Select(r => r.CoachRating!.Value).DefaultIfEmpty(6m).Average();
            var formScore = Math.Min(100m, Math.Max(0m, avgRating * 10m));

            var fitnessScore = 100m;
            var attendanceScore = attendanceRows.Count == 0
                ? 50m
                : (decimal)attendanceRows.Count(a => a.Attended) / attendanceRows.Count * 100m;

            var tacticalFit = player.PrimaryPosition switch
            {
                Position.Goalkeeper => 85m,
                Position.CentreBack or Position.DefensiveMidfielder => 80m,
                Position.CentralMidfielder => 78m,
                Position.LeftWinger or Position.RightWinger => 75m,
                Position.Striker or Position.CentreForward => 82m,
                _ => 70m
            };

            var opponentSuitability = 70m;
            var disciplinePenalty = recent.Sum(r => r.RedCards * 8 + r.YellowCards * 2);

            var total =
                (formScore * 0.30m) +
                (fitnessScore * 0.25m) +
                (attendanceScore * 0.15m) +
                (tacticalFit * 0.15m) +
                (opponentSuitability * 0.10m) -
                (disciplinePenalty * 0.05m);

            return new
            {
                Player = player,
                Score = Math.Round(total, 2),
                Factors = new
                {
                    Form = Math.Round(formScore, 1),
                    Fitness = Math.Round(fitnessScore, 1),
                    Attendance = Math.Round(attendanceScore, 1),
                    TacticalFit = Math.Round(tacticalFit, 1),
                    OpponentSuitability = Math.Round(opponentSuitability, 1),
                    DisciplinePenalty = disciplinePenalty
                }
            };
        }).OrderByDescending(s => s.Score).ToList();

        var goalkeeper = scored.FirstOrDefault(s => s.Player.PrimaryPosition == Position.Goalkeeper);
        var starters = new List<dynamic>();

        if (goalkeeper is not null)
        {
            starters.Add(new
            {
                goalkeeper.Player.Id,
                goalkeeper.Player.FullName,
                Position = goalkeeper.Player.PrimaryPosition.ToString(),
                goalkeeper.Score,
                goalkeeper.Factors
            });
        }

        var fieldPlayers = scored
            .Where(s => goalkeeper is null || s.Player.Id != goalkeeper.Player.Id)
            .Take(11 - starters.Count)
            .Select(s => new
            {
                s.Player.Id,
                s.Player.FullName,
                Position = s.Player.PrimaryPosition.ToString(),
                s.Score,
                s.Factors
            });

        starters.AddRange(fieldPlayers.Cast<dynamic>());

        var bench = scored.Skip(11).Take(7).Select(s => new
        {
            s.Player.Id,
            s.Player.FullName,
            Position = s.Player.PrimaryPosition.ToString(),
            s.Score
        }).ToList();

        var opponentRecent = await _fixtureRepository.QueryAsNoTracking()
            .Where(f => (f.HomeTeamId == opponentTeamId || f.AwayTeamId == opponentTeamId) && f.Status == MatchStatus.Finished)
            .OrderByDescending(f => f.KickOffUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        var opponentConceded = opponentRecent.Sum(f => f.HomeTeamId == opponentTeamId ? (f.AwayScore ?? 0) : (f.HomeScore ?? 0));

        var tacticalFocus = new List<string>();
        if (opponentConceded >= 7)
            tacticalFocus.Add("Opponent has conceded consistently; attack with early width and aggressive box entries.");
        else
            tacticalFocus.Add("Opponent defense is relatively stable; prioritize patient build-up and shot quality.");

        if (bench.Any(b => b.Position.Contains("Winger")))
            tacticalFocus.Add("Use wide substitutes after 60 minutes to target fatigue in fullback channels.");

        tacticalFocus.Add("Protect central areas in transition with at least one holding midfielder.");

        var formation = ChooseFormation(starters.Select(s => (string)s.Position).ToList());

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            FixtureId = fixtureId,
            PredictedOutcome = "Competitive",
            Confidence = 0.62m,
            ReasoningSummary = "Recommendation combines form, attendance, tactical fit, and availability.",
            GeneratedByVersion = "rules-v1"
        };

        await _predictionRepository.AddAsync(prediction, cancellationToken);
        await _predictionRepository.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            FixtureId = fixtureId,
            TeamId = targetTeamId,
            Formation = formation,
            StartingXI = starters,
            Bench = bench,
            TacticalFocus = tacticalFocus,
            RiskWarnings = new[]
            {
                "Recommendation confidence decreases when recent match/training data is sparse.",
                "Check late injury updates before finalizing lineup."
            },
            Confidence = prediction.Confidence,
            Explainability = "Score uses form (30%), fitness/readiness (25%), attendance (15%), tactical fit (15%), opponent suitability (10%), discipline penalty (5%)."
        });
    }

    private static string ChooseFormation(IReadOnlyCollection<string> starterPositions)
    {
        var wingerCount = starterPositions.Count(p => p.Contains("Winger"));
        var dmCount = starterPositions.Count(p => p.Contains("DefensiveMidfielder"));

        if (wingerCount >= 2) return "4-3-3";
        if (dmCount >= 2) return "4-2-3-1";
        return "4-4-2";
    }
}