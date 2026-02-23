using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        await SeedSymptomsAsync(context);
        // Fjernet SeedUsersAsync herfra - brugere oprettes nu i Program.cs
        await SeedPatientsAsync(context);
        await SeedSeizuresAsync(context);
    }

    private static async Task SeedSymptomsAsync(ApplicationDbContext context)
    {
        if (!context.Symptoms.Any())
        {
            var symptoms = new List<Symptom>
            {
                new() { Name = "Muskelstivhed", Description = "Stivhed i kroppen under anfald" },
                new() { Name = "Rykninger", Description = "Rytmiske muskelsammentrækninger" },
                new() { Name = "Øjenrulning", Description = "Øjnene ruller op i øjenhulerne" },
                new() { Name = "Tænderkast", Description = "Bider tænder sammen" },
                new() { Name = "Cyanose", Description = "Blåfarvning af læber eller fingre" },
                new() { Name = "Urinfrivillitet", Description = "Vandladning under anfald" },
                new() { Name = "Bevidstløshed", Description = "Midlertidigt bevidsthedstab" },
                new() { Name = "Forvirring", Description = "Forvirring efter anfald" }
            };

            await context.Symptoms.AddRangeAsync(symptoms);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPatientsAsync(ApplicationDbContext context)
    {
        // Patienter oprettes nu primært i Program.cs, men vi beholder denne som backup
        // eller hvis vi skal tilføje flere testpatienter senere
        if (!context.Patients.Any(p => p.PatientId == "P003"))
        {
            var patient3 = new Patient
            {
                PatientId = "P003",
                FirstName = "Peter",
                LastName = "Rasmussen",
                DateOfBirth = new DateTime(1978, 11, 8),
                Gender = Gender.Male,
                Diagnosis = "Absence epilepsi",
                Notes = "Sjældne anfald, stabil medicinering",
                CreatedAt = DateTime.UtcNow
            };

            await context.Patients.AddAsync(patient3);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSeizuresAsync(ApplicationDbContext context)
    {
        if (!context.Seizures.Any())
        {
            var lars = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == "P001");
            var mette = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == "P002");

            if (lars != null)
            {
                var seizures = new List<Seizure>
                {
                    new()
                    {
                        PatientId = lars.Id,
                        StartTime = DateTime.UtcNow.AddDays(-5),
                        EndTime = DateTime.UtcNow.AddDays(-5).AddMinutes(2),
                        Type = SeizureType.FocalImpaired,
                        ConsciousnessLoss = true,
                        Notes = "Morgen anfald, observeret af pårørende",
                        RegisteredByUserId = "seed",
                        RegisteredByName = "System Seed",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        PatientId = lars.Id,
                        StartTime = DateTime.UtcNow.AddDays(-2),
                        EndTime = DateTime.UtcNow.AddDays(-2).AddMinutes(1),
                        Type = SeizureType.FocalAware,
                        ConsciousnessLoss = false,
                        Notes = "Let anfald, kort varighed",
                        RegisteredByUserId = "seed",
                        RegisteredByName = "System Seed",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Seizures.AddRangeAsync(seizures);
            }

            if (mette != null)
            {
                var seizures = new List<Seizure>
                {
                    new()
                    {
                        PatientId = mette.Id,
                        StartTime = DateTime.UtcNow.AddDays(-10),
                        EndTime = DateTime.UtcNow.AddDays(-10).AddMinutes(3),
                        Type = SeizureType.TonicClonic,
                        ConsciousnessLoss = true,
                        Notes = "Alvorligt anfald, krævede assistance",
                        RegisteredByUserId = "seed",
                        RegisteredByName = "System Seed",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Seizures.AddRangeAsync(seizures);
            }

            await context.SaveChangesAsync();
        }
    }
}