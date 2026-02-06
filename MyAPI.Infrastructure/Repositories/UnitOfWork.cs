using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;
using MyAPI.Infrastructure.Data;

namespace MyAPI.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private Repository<Patient>? _patients;
    private Repository<Seizure>? _seizures;
    private Repository<Medication>? _medications;
    private Repository<Symptom>? _symptoms;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Patient> Patients => _patients ??= new Repository<Patient>(_context);
    public IRepository<Seizure> Seizures => _seizures ??= new Repository<Seizure>(_context);
    public IRepository<Medication> Medications => _medications ??= new Repository<Medication>(_context);
    public IRepository<Symptom> Symptoms => _symptoms ??= new Repository<Symptom>(_context);

    public async Task<int> SaveChangesAsync()
    {
        var entries = _context.ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}