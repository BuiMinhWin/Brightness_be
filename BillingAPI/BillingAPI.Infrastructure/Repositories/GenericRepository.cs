using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BillingAPI.Infrastructure.Data;
using BrightNess.Library.Interface;
using BrightNess.Library.Responses;
using Microsoft.EntityFrameworkCore;

namespace BillingAPI.Infrastructure.Repositories
{
    public class GenericRepository<T>(BillingDbContext context) : IGenericInterface<T> where T : class
    {
        protected readonly BillingDbContext Context = context;
        protected readonly DbSet<T> DbSet = context.Set<T>();

        public virtual async Task<Response> CreateAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            var changes = await Context.SaveChangesAsync();
            return changes > 0
                ? new Response(true, "Entity created successfully")
                : new Response(false, "No changes were saved");
        }

        public virtual async Task<Response> UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            var changes = await Context.SaveChangesAsync();
            return changes > 0
                ? new Response(true, "Entity updated successfully")
                : new Response(false, "No changes were saved");
        }

        public virtual async Task<Response> DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            var changes = await Context.SaveChangesAsync();
            return changes > 0
                ? new Response(true, "Entity deleted successfully")
                : new Response(false, "No changes were saved");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().ToListAsync();
        }

        public virtual async Task<T?> FindByIdAsync(Guid id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
        }
    }
}
