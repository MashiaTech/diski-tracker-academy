using DiskiTrack.Contracts.ViewModels.Players;
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
public sealed class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGenericRepository<Team> _teamRepository;

    public PlayersController(IPlayerRepository playerRepository, IGenericRepository<Team> teamRepository)
    {
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Parent + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<IReadOnlyList<PlayerViewModel>>> GetAll([FromQuery] Guid? teamId, CancellationToken cancellationToken)
    {
        var query = _playerRepository.QueryAsNoTracking();
        if (teamId is not null)
            query = query.Where(p => p.TeamId == teamId.Value);

        var players = await query
            .OrderBy(p => p.FullName)
            .Select(p => new PlayerViewModel
            {
                Id = p.Id,
                FullName = p.FullName
            })
            .ToListAsync(cancellationToken);

        return Ok(players);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst + "," + AppRoles.Player + "," + AppRoles.Parent + "," + AppRoles.Scout + "," + AppRoles.LeagueAdmin)]
    public async Task<ActionResult<PlayerViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.QueryAsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PlayerViewModel
            {
                Id = p.Id,
                FullName = p.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);

        return player is null ? NotFound() : Ok(player);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<PlayerViewModel>> Create(CreatePlayerRequest request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == request.TeamId)
            .Select(t => new { t.Id, t.TenantId })
            .FirstOrDefaultAsync(cancellationToken);

        if (team is null)
            return BadRequest("Team does not exist.");

        var player = new Player
        {
            Id = Guid.NewGuid(),
            TenantId = team.TenantId,
            TeamId = request.TeamId,
            FullName = request.FullName,
            JerseyNumber = request.JerseyNumber,
            DateOfBirth = request.DateOfBirth,
            PrimaryPosition = request.PrimaryPosition,
            SecondaryPosition = request.SecondaryPosition,
            DominantFoot = request.DominantFoot,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            SchoolGrade = request.SchoolGrade,
            GuardianName = request.GuardianName,
            GuardianPhone = request.GuardianPhone,
            IsActive = true
        };

        await _playerRepository.AddAsync(player, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = player.Id }, new PlayerViewModel
        {
            Id = player.Id,
            FullName = player.FullName
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach)]
    public async Task<ActionResult<PlayerViewModel>> Update(Guid id, UpdatePlayerRequest request, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(id, cancellationToken);
        if (player is null)
            return NotFound();

        player.FullName = request.FullName;
        player.JerseyNumber = request.JerseyNumber;
        player.DateOfBirth = request.DateOfBirth;
        player.PrimaryPosition = request.PrimaryPosition;
        player.SecondaryPosition = request.SecondaryPosition;
        player.DominantFoot = request.DominantFoot;
        player.HeightCm = request.HeightCm;
        player.WeightKg = request.WeightKg;
        player.SchoolGrade = request.SchoolGrade;
        player.GuardianName = request.GuardianName;
        player.GuardianPhone = request.GuardianPhone;
        player.IsActive = request.IsActive;

        _playerRepository.Update(player);
        await _playerRepository.SaveChangesAsync(cancellationToken);

        return Ok(new PlayerViewModel
        {
            Id = player.Id,
            FullName = player.FullName
        });
    }

    public sealed class CreatePlayerRequest
    {
        public Guid TeamId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Position PrimaryPosition { get; set; } = Position.Unknown;
        public Position? SecondaryPosition { get; set; }
        public DominantFoot DominantFoot { get; set; } = DominantFoot.Unknown;
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        public string? SchoolGrade { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
    }

    public sealed class UpdatePlayerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Position PrimaryPosition { get; set; } = Position.Unknown;
        public Position? SecondaryPosition { get; set; }
        public DominantFoot DominantFoot { get; set; } = DominantFoot.Unknown;
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        public string? SchoolGrade { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
        public bool IsActive { get; set; } = true;
    }
}