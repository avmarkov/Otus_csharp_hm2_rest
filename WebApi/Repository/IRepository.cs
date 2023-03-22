using WebApi.Models;

namespace WebApi.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetAsync(long id);

        Task AddAsync(T entity);
    }
}
