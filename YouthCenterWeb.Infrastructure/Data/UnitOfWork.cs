using Microsoft.AspNetCore.Authorization.Infrastructure;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public IGenericRepo<Role>? RolesRepo;
    public IGenericRepo<Activity>? ActivitiesRepo;

    public IGenericRepo<Role> GetRolesRepo
    {
        get
        {
            if (RolesRepo == null) { RolesRepo = new GenericRepo<Role>(_context); }
            return RolesRepo;
        }

    }

    public IGenericRepo<Activity> GetActivitiesRepo
    {
        get
        {
            if (ActivitiesRepo == null) { ActivitiesRepo = new GenericRepo<Activity>(_context); }
            return ActivitiesRepo;
        }

    }
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}