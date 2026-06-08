using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.DataAccess.Repository.DbContext;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<FixtureEvent> FixtureEvents => Set<FixtureEvent>();
    public DbSet<FixtureStats> FixtureStats => Set<FixtureStats>();
    public DbSet<Lineup> Lineups => Set<Lineup>();
    public DbSet<LineupPlayer> LineupPlayers => Set<LineupPlayer>();
    public DbSet<PlayerMatchStats> PlayerMatchStats => Set<PlayerMatchStats>();
    public DbSet<PlayerAvailability> PlayerAvailabilities => Set<PlayerAvailability>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingAttendance> TrainingAttendance => Set<TrainingAttendance>();
    public DbSet<InjuryRecord> InjuryRecords => Set<InjuryRecord>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tenant 
        builder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).HasMaxLength(200).IsRequired();
            e.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            e.Property(t => t.ContactEmail).HasMaxLength(320);
        });

        // Season 
        builder.Entity<Season>(e =>
        {
            e.HasQueryFilter(s => _tenantContext.TenantId == null || s.TenantId == _tenantContext.TenantId);
            e.Property(s => s.Name).HasMaxLength(100).IsRequired();
            e.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        // Competition 
        builder.Entity<Competition>(e =>
        {
            e.HasQueryFilter(c => _tenantContext.TenantId == null || c.TenantId == _tenantContext.TenantId);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Season).WithMany(s => s.Competitions).HasForeignKey(c => c.SeasonId).OnDelete(DeleteBehavior.Cascade);
        });

        // Team 
        builder.Entity<Team>(e =>
        {
            e.HasQueryFilter(t => _tenantContext.TenantId == null || t.TenantId == _tenantContext.TenantId);
            e.Property(t => t.Name).HasMaxLength(200).IsRequired();
            e.Property(t => t.AgeGroup).HasMaxLength(50);
            e.Property(t => t.HomeVenue).HasMaxLength(200);
            e.HasOne(t => t.Tenant).WithMany(tn => tn.Teams).HasForeignKey(t => t.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        //  Player 
        builder.Entity<Player>(e =>
        {
            e.HasQueryFilter(p => _tenantContext.TenantId == null || p.TenantId == _tenantContext.TenantId);
            e.Property(p => p.FullName).HasMaxLength(200).IsRequired();
            e.Property(p => p.SchoolGrade).HasMaxLength(20);
            e.Property(p => p.GuardianName).HasMaxLength(200);
            e.Property(p => p.GuardianPhone).HasMaxLength(30);
            e.HasOne(p => p.Team).WithMany(t => t.Players).HasForeignKey(p => p.TeamId).OnDelete(DeleteBehavior.Restrict);
        });

        //  Fixture 
        builder.Entity<Fixture>(e =>
        {
            e.HasQueryFilter(f => _tenantContext.TenantId == null || f.TenantId == _tenantContext.TenantId);
            e.Property(f => f.Venue).HasMaxLength(200);
            e.HasOne(f => f.Competition).WithMany(c => c.Fixtures).HasForeignKey(f => f.CompetitionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.HomeTeam).WithMany().HasForeignKey(f => f.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.AwayTeam).WithMany().HasForeignKey(f => f.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Stats).WithOne(s => s.Fixture).HasForeignKey<FixtureStats>(s => s.FixtureId);
            e.HasMany(f => f.Events).WithOne(ev => ev.Fixture).HasForeignKey(ev => ev.FixtureId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(f => f.PlayerStats).WithOne(ps => ps.Fixture).HasForeignKey(ps => ps.FixtureId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(f => f.Availabilities).WithOne(a => a.Fixture).HasForeignKey(a => a.FixtureId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(f => f.Lineups).WithOne(l => l.Fixture).HasForeignKey(l => l.FixtureId).OnDelete(DeleteBehavior.Cascade);
        });

        //  FixtureEvent 
        builder.Entity<FixtureEvent>(e =>
        {
            e.Property(ev => ev.Notes).HasMaxLength(500);
            e.HasOne(ev => ev.Player).WithMany().HasForeignKey(ev => ev.PlayerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(ev => ev.SecondaryPlayer).WithMany().HasForeignKey(ev => ev.SecondaryPlayerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(ev => ev.Team).WithMany().HasForeignKey(ev => ev.TeamId).OnDelete(DeleteBehavior.Restrict);
        });

        //  Lineup / LineupPlayer 
        builder.Entity<Lineup>(e =>
        {
            e.HasIndex(l => new { l.FixtureId, l.TeamId }).IsUnique();
            e.Property(l => l.Formation).HasMaxLength(20);
            e.HasOne(l => l.Team).WithMany().HasForeignKey(l => l.TeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(l => l.Players).WithOne(lp => lp.Lineup).HasForeignKey(lp => lp.LineupId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LineupPlayer>(e =>
        {
            e.HasIndex(lp => new { lp.LineupId, lp.PlayerId }).IsUnique();
            e.HasOne(lp => lp.Player).WithMany().HasForeignKey(lp => lp.PlayerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── PlayerMatchStats ──────────────────────────────────────────────────
        builder.Entity<PlayerMatchStats>(e =>
        {
            e.HasIndex(ps => new { ps.FixtureId, ps.PlayerId }).IsUnique();
            e.Property(ps => ps.CoachRating).HasPrecision(4, 1);
            e.HasOne(ps => ps.Player).WithMany(p => p.MatchStats).HasForeignKey(ps => ps.PlayerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ps => ps.Team).WithMany().HasForeignKey(ps => ps.TeamId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── PlayerAvailability ────────────────────────────────────────────────
        builder.Entity<PlayerAvailability>(e =>
        {
            e.HasIndex(a => new { a.FixtureId, a.PlayerId }).IsUnique();
            e.Property(a => a.Reason).HasMaxLength(500);
            e.HasOne(a => a.Player).WithMany(p => p.Availabilities).HasForeignKey(a => a.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── TrainingSession ───────────────────────────────────────────────────
        builder.Entity<TrainingSession>(e =>
        {
            e.HasQueryFilter(ts => _tenantContext.TenantId == null || ts.TenantId == _tenantContext.TenantId);
            e.Property(ts => ts.Focus).HasMaxLength(200);
            e.HasOne(ts => ts.Team).WithMany().HasForeignKey(ts => ts.TeamId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(ts => ts.Attendance).WithOne(a => a.TrainingSession).HasForeignKey(a => a.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── TrainingAttendance ────────────────────────────────────────────────
        builder.Entity<TrainingAttendance>(e =>
        {
            e.HasIndex(a => new { a.TrainingSessionId, a.PlayerId }).IsUnique();
            e.HasOne(a => a.Player).WithMany(p => p.TrainingAttendance).HasForeignKey(a => a.PlayerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── InjuryRecord ──────────────────────────────────────────────────────
        builder.Entity<InjuryRecord>(e =>
        {
            e.Property(ir => ir.InjuryType).HasMaxLength(200).IsRequired();
            e.Property(ir => ir.BodyPart).HasMaxLength(100);
            e.HasOne(ir => ir.Player).WithMany(p => p.Injuries).HasForeignKey(ir => ir.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Prediction ────────────────────────────────────────────────────────
        builder.Entity<Prediction>(e =>
        {
            e.Property(p => p.Confidence).HasPrecision(5, 4);
            e.Property(p => p.PredictedOutcome).HasMaxLength(20).IsRequired();
            e.Property(p => p.ReasoningSummary).HasMaxLength(2000);
            e.HasOne(p => p.Fixture).WithMany().HasForeignKey(p => p.FixtureId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── ApplicationUser ───────────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Models.Base.AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAtUtc = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
