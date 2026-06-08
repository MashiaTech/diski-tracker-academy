using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Repository.DbContext;
using DiskiTrack.DataAccess.Repository.Interfaces;

namespace DiskiTrack.DataAccess.Repository.Repositories;

public sealed class FixtureRepository : GenericRepository<Fixture>, IFixtureRepository
{
    public FixtureRepository(AppDbContext context) : base(context) { }
}
