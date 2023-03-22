using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Repository
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly DbContext datacontext;
        public EfRepository(DbContext datacontext)
        {
            this.datacontext = datacontext;
        }

        public async Task AddAsync(T entity)
        {            
            await datacontext.Set<T>().AddAsync(entity);
            await datacontext.SaveChangesAsync();
        }

        public async Task<T?> GetAsync(long id)
        {
            T? entity = null;

            entity = await datacontext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

            return entity;
        }
    }
}
