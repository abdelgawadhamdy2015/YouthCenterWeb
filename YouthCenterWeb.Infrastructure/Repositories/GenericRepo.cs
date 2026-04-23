using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using YouthCenterWeb.Data;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class GenericRepo<T> : IGenericRepo<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepo(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    // جلب الكل مع إمكانية إضافة Include
    public async Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.AsNoTracking().ToListAsync();
    }

    // جلب عنصر واحد عن طريق ID مع Include
    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // ملاحظة: نفترض أن اسم حقل المفتاح هو "Id" في جميع الجداول
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}