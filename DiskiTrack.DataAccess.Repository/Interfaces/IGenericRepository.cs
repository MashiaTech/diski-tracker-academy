using System.Linq.Expressions;

namespace DiskiTrack.DataAccess.Repository.Interfaces;

public interface IGenericRepository<T> : IRepositoryBase<T> where T : class
{
}
