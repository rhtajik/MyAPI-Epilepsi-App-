using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace MyAPI.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        await SeedSymptomsAsync(context);
        await SeedUsersAsync(userManager);
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

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin bruger
        if (await userManager.FindByEmailAsync("admin@myapi.dk") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@myapi.dk",
                Email = "admin@myapi.dk",
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Admin,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
        }

        // Sygeplejerske
        if (await userManager.FindByEmailAsync("susanne@hospital.dk") == null)
        {
            var nurse = new ApplicationUser
            {
                UserName = "susanne@hospital.dk",
                Email = "susanne@hospital.dk",
                FirstName = "Susanne",
                LastName = "Jensen",
                Role = UserRole.Nurse,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(nurse, "Nurse123!");
        }
    }

    private static async Task SeedPatientsAsync(ApplicationDbContext context)
    {
        if (!context.Patients.Any())
        {
            var patients = new List<Patient>
            {
                new()
                {
                    PatientId = "P001",
                    FirstName = "Lars",
                    LastName = "Hansen",
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Gender = Gender.Male,
                    Diagnosis = "Epilepsi med fokale anfald",
                    Notes = "Patient siden 2020, god effekt af medicin"
                },
                new()
                {
                    PatientId = "P002",
                    FirstName = "Mette",
                    LastName = "Nielsen",
                    DateOfBirth = new DateTime(1992, 7, 22),
                    Gender = Gender.Female,
                    Diagnosis = "Generaliseret epilepsi",
                    Notes = "Hyppige anfald, under observation"
                },
                new()
                {
                    PatientId = "P003",
                    FirstName = "Peter",
                    LastName = "Rasmussen",
                    DateOfBirth = new DateTime(1978, 11, 8),
                    Gender = Gender.Male,
                    Diagnosis = "Absence epilepsi",
                    Notes = "Sjældne anfald, stabil medicinering"
                }
            };

            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSeizuresAsync(ApplicationDbContext context)
    {
        if (!context.Seizures.Any())
        {
            var lars = await context.Patients.FindAsync(1);
            var mette = await context.Patients.FindAsync(2);

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
                        RegisteredByName = "System Seed"
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
                        RegisteredByName = "System Seed"
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
                        RegisteredByName = "System Seed"
                    }
                };

                await context.Seizures.AddRangeAsync(seizures);
            }

            await context.SaveChangesAsync();
        }
    }
}