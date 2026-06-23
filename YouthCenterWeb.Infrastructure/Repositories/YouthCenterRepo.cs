using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

using YouthCenterWeb.Domain.Entities;

public class YouthCenterRepo(AppDbContext context) : IYouthCenterRepo
{
    private readonly AppDbContext _context = context;

    // ── shared include  
    private IQueryable<YouthCenter> WithRelations() =>
        _context.YouthCenters
            .Include(y => y.YouthCenterActivities)
                .ThenInclude(yca => yca.Activity);

    // ── queries 
    public Task<List<YouthCenter>> GetAllAsync() =>
        WithRelations().ToListAsync();

    public Task<YouthCenter?> GetByIdAsync(int id) =>
        WithRelations().FirstOrDefaultAsync(y => y.Id == id);

    public async Task AddAsync(YouthCenter entity) =>
        await _context.YouthCenters.AddAsync(entity);

    public void Update(YouthCenter entity) =>
        _context.YouthCenters.Update(entity);

    public void Delete(YouthCenter entity) =>
        _context.YouthCenters.Remove(entity);

    public Task SaveChangesAsync() =>
        _context.SaveChangesAsync();
}