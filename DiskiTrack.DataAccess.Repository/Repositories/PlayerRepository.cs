using DiskiTrack.DataAccess.Models.Entities;
using DiskiTrack.DataAccess.Repository.DbContext;
using DiskiTrack.DataAccess.Repository.Interfaces;

namespace DiskiTrack.DataAccess.Repository.Repositories;

public sealed class PlayerRepository : GenericRepository<Player>, IPlayerRepository
{
    public PlayerRepository(AppDbContext context) : base(context) { }
}
