using MyAPI.Core.Entities;

namespace MyAPI.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Patient> Patients { get; }
    IRepository<Seizure> Seizures { get; }
    IRepository<Medication> Medications { get; }
    IRepository<Symptom> Symptoms { get; }
    Task<int> SaveChangesAsync();
}