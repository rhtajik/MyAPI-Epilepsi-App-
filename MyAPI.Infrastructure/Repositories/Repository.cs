using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Interfaces;
using MyAPI.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void SoftDelete(T entity)
    {
        var property = entity.GetType().GetProperty("IsDeleted");
        if (property != null)
        {
            property.SetValue(entity, true);
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity); // Fallback til hard delete hvis ingen soft delete
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
}