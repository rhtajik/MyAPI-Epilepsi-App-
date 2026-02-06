using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Seizure> Seizures { get; set; }
    public DbSet<Symptom> Symptoms { get; set; }
    public DbSet<SeizureSymptom> SeizureSymptoms { get; set; }
    public DbSet<Medication> Medications { get; set; }
    // FJERN: public DbSet<PatientRelative> PatientRelatives { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Many-to-many: Seizure <-> Symptom
        builder.Entity<SeizureSymptom>()
            .HasKey(ss => new { ss.SeizureId, ss.SymptomId });

        builder.Entity<SeizureSymptom>()
            .HasOne(ss => ss.Seizure)
            .WithMany(s => s.SeizureSymptoms)
            .HasForeignKey(ss => ss.SeizureId);

        builder.Entity<SeizureSymptom>()
            .HasOne(ss => ss.Symptom)
            .WithMany(s => s.SeizureSymptoms)
            .HasForeignKey(ss => ss.SymptomId);

        // FJERN: Al konfiguration om PatientRelative

        // Global query filters
        builder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Seizure>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<Medication>().HasQueryFilter(m => !m.IsDeleted);

        // Indeks
        builder.Entity<Seizure>()
            .HasIndex(s => new { s.PatientId, s.StartTime });

        builder.Entity<Patient>()
            .HasIndex(p => p.PatientId)
            .IsUnique();
    }
}