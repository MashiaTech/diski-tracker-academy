using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Models.Enums;
using DiskiTrack.DataAccess.Repository.Interfaces;
using DiskiTrack.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/training-sessions")]
public sealed class TrainingSessionsController : ControllerBase
{
    private readonly IGenericRepository<TrainingSession> _trainingSessionRepository;
    private readonly IGenericRepository<TrainingAttendance> _attendanceRepository;
    private readonly IGenericRepository<Team> _teamRepository;

    public TrainingSessionsController(
        IGenericRepository<TrainingSession> trainingSessionRepository,
        IGenericRepository<TrainingAttendance> attendanceRepository,
        IGenericRepository<Team> teamRepository)
    {
        _trainingSessionRepository = trainingSessionRepository;
        _attendanceRepository = attendanceRepository;
        _teamRepository = teamRepository;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst)]
    public async Task<ActionResult> List([FromQuery] Guid? teamId, CancellationToken cancellationToken)
    {
        var query = _trainingSessionRepository.QueryAsNoTracking();
        if (teamId is not null)
            query = query.Where(s => s.TeamId == teamId.Value);

        var sessions = await query
            .OrderByDescending(s => s.ScheduledAtUtc)
            .Select(s => new
            {
                s.Id,
                s.TeamId,
                s.ScheduledAtUtc,
                s.DurationMinutes,
                Intensity = s.Intensity.ToString(),
                s.Focus,
                AttendanceCount = s.Attendance.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(sessions);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst)]
    public async Task<ActionResult> Create(CreateTrainingSessionRequest request, CancellationToken cancellationToken)
    {
        var teamExists = await _teamRepository.AnyAsync(t => t.Id == request.TeamId, cancellationToken);
        if (!teamExists)
            return BadRequest("Team does not exist.");

        var tenantId = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == request.TeamId)
            .Select(t => t.TenantId)
            .FirstAsync(cancellationToken);

        var session = new TrainingSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TeamId = request.TeamId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            Intensity = request.Intensity,
            Focus = request.Focus,
            Notes = request.Notes,
            CoachUserId = request.CoachUserId
        };

        await _trainingSessionRepository.AddAsync(session, cancellationToken);
        await _trainingSessionRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(List), new { teamId = request.TeamId }, new
        {
            session.Id,
            session.TeamId,
            session.ScheduledAtUtc,
            session.DurationMinutes,
            Intensity = session.Intensity.ToString(),
            session.Focus
        });
    }

    [HttpPost("{id:guid}/attendance")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.TenantAdmin + "," + AppRoles.Coach + "," + AppRoles.Analyst)]
    public async Task<ActionResult> CaptureAttendance(Guid id, CaptureAttendanceRequest request, CancellationToken cancellationToken)
    {
        var session = await _trainingSessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null)
            return NotFound("Training session not found.");

        if (request.Entries.Count == 0)
            return BadRequest("Attendance entries are required.");

        var playerIds = request.Entries.Select(e => e.PlayerId).Distinct().ToArray();
        var teamPlayers = await _teamRepository.QueryAsNoTracking()
            .Where(t => t.Id == session.TeamId)
            .SelectMany(t => t.Players)
            .Where(p => playerIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (teamPlayers.Count != playerIds.Length)
            return BadRequest("One or more players are not in this team.");

        var existing = await _attendanceRepository.FindAsync(a => a.TrainingSessionId == id, cancellationToken);
        if (existing.Count > 0)
            _attendanceRepository.RemoveRange(existing);

        var attendanceRows = request.Entries.Select(e => new TrainingAttendance
        {
            Id = Guid.NewGuid(),
            TrainingSessionId = id,
            PlayerId = e.PlayerId,
            Attended = e.Attended,
            Rpe = e.Rpe,
            CoachRating = e.CoachRating,
            Notes = e.Notes
        }).ToList();

        await _attendanceRepository.AddRangeAsync(attendanceRows, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Attendance saved.", count = attendanceRows.Count });
    }

    public sealed class CreateTrainingSessionRequest
    {
        public Guid TeamId { get; set; }
        public DateTime ScheduledAtUtc { get; set; }
        public int DurationMinutes { get; set; }
        public TrainingIntensity Intensity { get; set; } = TrainingIntensity.Medium;
        public string? Focus { get; set; }
        public string? Notes { get; set; }
        public string? CoachUserId { get; set; }
    }

    public sealed class CaptureAttendanceRequest
    {
        public List<AttendanceEntry> Entries { get; set; } = [];
    }

    public sealed class AttendanceEntry
    {
        public Guid PlayerId { get; set; }
        public bool Attended { get; set; }
        public int? Rpe { get; set; }
        public int? CoachRating { get; set; }
        public string? Notes { get; set; }
    }
}