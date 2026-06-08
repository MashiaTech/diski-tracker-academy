using DiskiTrack.Contracts.ViewModels.Matches;
using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Models.Enums;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FixturesController : ControllerBase
{
    private readonly IFixtureRepository _fixtureRepository;
    private readonly IGenericRepository<FixtureEvent> _fixtureEventRepository;
    private readonly IGenericRepository<Lineup> _lineupRepository;
    private readonly IGenericRepository<LineupPlayer> _lineupPlayerRepository;
    private readonly IGenericRepository<PlayerAvailability> _availabilityRepository;
    private readonly IGenericRepository<Competition> _competitionRepository;
    private readonly IGenericRepository<Team> _teamRepository;

    public FixturesController(
        IFixtureRepository fixtureRepository,
        IGenericRepository<FixtureEvent> fixtureEventRepository,
        IGenericRepository<Lineup> lineupRepository,
        IGenericRepository<LineupPlayer> lineupPlayerRepository,
        IGenericRepository<PlayerAvailability> availabilityRepository,
        IGenericRepository<Competition> competitionRepository,
        IGenericRepository<Team> teamRepository)
    {
        _fixtureRepository = fixtureRepository;
        _fixtureEventRepository = fixtureEventRepository;
        _lineupRepository = lineupRepository;
        _lineupPlayerRepository = lineupPlayerRepository;
        _availabilityRepository = availabilityRepository;
        _competitionRepository = competitionRepository;
        _teamRepository = teamRepository;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<IReadOnlyList<MatchViewModel>>> GetAll(CancellationToken cancellationToken)
    {
        var fixtures = await _fixtureRepository.QueryAsNoTracking()
            .OrderByDescending(f => f.KickOffUtc)
            .Select(f => new MatchViewModel
            {
                Id = f.Id,
                KickOffUtc = f.KickOffUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(fixtures);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<MatchViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var fixture = await _fixtureRepository.QueryAsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new MatchViewModel
            {
                Id = f.Id,
                KickOffUtc = f.KickOffUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        return fixture is null ? NotFound() : Ok(fixture);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<MatchViewModel>> Create(CreateFixtureRequest request, CancellationToken cancellationToken)
    {
        if (request.HomeTeamId == request.AwayTeamId)
            return BadRequest("Home and away teams must be different.");

        var competition = await _competitionRepository.GetByIdAsync(request.CompetitionId, cancellationToken);
        if (competition is null)
            return BadRequest("Competition does not exist.");

        var homeTeamExists = await _teamRepository.AnyAsync(t => t.Id == request.HomeTeamId, cancellationToken);
        var awayTeamExists = await _teamRepository.AnyAsync(t => t.Id == request.AwayTeamId, cancellationToken);
        if (!homeTeamExists || !awayTeamExists)
            return BadRequest("One or both teams do not exist.");

        var fixture = new Fixture
        {
            Id = Guid.NewGuid(),
            TenantId = competition.TenantId,
            CompetitionId = request.CompetitionId,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            KickOffUtc = request.KickOffUtc,
            Venue = request.Venue,
            Status = MatchStatus.Scheduled,
            Notes = request.Notes
        };

        await _fixtureRepository.AddAsync(fixture, cancellationToken);
        await _fixtureRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = fixture.Id }, new MatchViewModel
        {
            Id = fixture.Id,
            KickOffUtc = fixture.KickOffUtc
        });
    }

    [HttpPost("{id:guid}/events")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult> AddEvents(Guid id, AddFixtureEventsRequest request, CancellationToken cancellationToken)
    {
        var fixture = await _fixtureRepository.GetByIdAsync(id, cancellationToken);
        if (fixture is null)
            return NotFound("Fixture not found.");

        if (request.Events.Count == 0)
            return BadRequest("At least one event is required.");

        var events = request.Events.Select(e => new FixtureEvent
        {
            Id = Guid.NewGuid(),
            FixtureId = id,
            TeamId = e.TeamId,
            PlayerId = e.PlayerId,
            SecondaryPlayerId = e.SecondaryPlayerId,
            Type = e.Type,
            Minute = e.Minute,
            CardType = e.CardType,
            IsHomeTeam = e.TeamId == fixture.HomeTeamId,
            Notes = e.Notes
        }).ToList();

        await _fixtureEventRepository.AddRangeAsync(events, cancellationToken);
        await _fixtureEventRepository.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Fixture events recorded.", count = events.Count });
    }

    [HttpPost("{id:guid}/lineup")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult> SubmitLineup(Guid id, SubmitLineupRequest request, CancellationToken cancellationToken)
    {
        var fixture = await _fixtureRepository.GetByIdAsync(id, cancellationToken);
        if (fixture is null)
            return NotFound("Fixture not found.");

        var validTeam = request.TeamId == fixture.HomeTeamId || request.TeamId == fixture.AwayTeamId;
        if (!validTeam)
            return BadRequest("Team is not part of this fixture.");

        var playerIds = request.Players.Select(p => p.PlayerId).Distinct().ToArray();
        var teamPlayerCount = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == request.TeamId)
            .SelectMany(t => t.Players)
            .CountAsync(p => playerIds.Contains(p.Id), cancellationToken);

        if (teamPlayerCount != playerIds.Length)
            return BadRequest("One or more players do not belong to the selected team.");

        var existingLineup = await _lineupRepository.FirstOrDefaultAsync(
            l => l.FixtureId == id && l.TeamId == request.TeamId,
            cancellationToken);

        if (existingLineup is not null)
        {
            var existingPlayers = await _lineupPlayerRepository.FindAsync(lp => lp.LineupId == existingLineup.Id, cancellationToken);
            _lineupPlayerRepository.RemoveRange(existingPlayers);
            _lineupRepository.Remove(existingLineup);
            await _lineupRepository.SaveChangesAsync(cancellationToken);
        }

        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            FixtureId = id,
            TeamId = request.TeamId,
            Formation = request.Formation
        };

        await _lineupRepository.AddAsync(lineup, cancellationToken);

        var lineupPlayers = request.Players.Select(p => new LineupPlayer
        {
            Id = Guid.NewGuid(),
            LineupId = lineup.Id,
            PlayerId = p.PlayerId,
            Position = p.Position,
            IsStarting = p.IsStarting,
            SubstitutedInMinute = p.SubstitutedInMinute,
            SubstitutedOutMinute = p.SubstitutedOutMinute
        }).ToList();

        await _lineupPlayerRepository.AddRangeAsync(lineupPlayers, cancellationToken);

        var availability = request.Players.Select(p => new PlayerAvailability
        {
            Id = Guid.NewGuid(),
            FixtureId = id,
            PlayerId = p.PlayerId,
            Status = AvailabilityStatus.Available,
            Reason = "Included in submitted lineup"
        }).ToList();

        await _availabilityRepository.AddRangeAsync(availability, cancellationToken);
        await _lineupRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Lineup submitted.", lineupId = lineup.Id, playerCount = lineupPlayers.Count });
    }

    public sealed class CreateFixtureRequest
    {
        public Guid CompetitionId { get; set; }
        public Guid HomeTeamId { get; set; }
        public Guid AwayTeamId { get; set; }
        public DateTime KickOffUtc { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public sealed class AddFixtureEventsRequest
    {
        public List<FixtureEventRequestItem> Events { get; set; } = [];
    }

    public sealed class FixtureEventRequestItem
    {
        public Guid TeamId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? SecondaryPlayerId { get; set; }
        public EventType Type { get; set; }
        public int Minute { get; set; }
        public CardType? CardType { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class SubmitLineupRequest
    {
        public Guid TeamId { get; set; }
        public string Formation { get; set; } = "4-3-3";
        public List<LineupPlayerRequestItem> Players { get; set; } = [];
    }

    public sealed class LineupPlayerRequestItem
    {
        public Guid PlayerId { get; set; }
        public Position Position { get; set; }
        public bool IsStarting { get; set; } = true;
        public int? SubstitutedInMinute { get; set; }
        public int? SubstitutedOutMinute { get; set; }
    }
}