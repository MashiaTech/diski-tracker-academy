using DiskiTrack.DataAccess.Repository.DbContext;
using DiskiTrack.DataAccess.Repository.Interfaces;

namespace DiskiTrack.DataAccess.Repository.Repositories;

public class GenericRepository<T> : RepositoryBase<T>, IGenericRepository<T> where T : class
{
    public GenericRepository(AppDbContext context) : base(context) { }
}
