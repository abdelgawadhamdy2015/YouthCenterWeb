using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class CupRepo(AppDbContext appContext) : ICupRepo
{
    private readonly AppDbContext context = appContext;

    private IQueryable<Cup> WithRelations() =>
            context.Cups
               .Include(x => x.Activity)
               .Include(x => x.YouthCenter);
    public async Task AddAsync(Cup entity) =>
        await context.Cups.AddAsync(entity);



    public async Task DeleteAsync(Cup entity) => context.Cups.Remove(entity);


    public async Task<List<Cup>?> GetAllAsync(params Expression<Func<Cup, object>>[] includes) => await WithRelations().ToListAsync();

    public async Task<Cup?> GetByIdAsync(int? id, params Expression<Func<Cup, object>>[] includes) => await WithRelations().FirstOrDefaultAsync(x => x.Id == id);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();



    public async Task UpdateAsync(Cup entity) => context.Cups.Update(entity);

}