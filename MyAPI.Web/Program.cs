using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;
using MyAPI.Infrastructure.Repositories;
using MyAPI.Infrastructure.Services;
using MyAPI.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie configuration - START på Login, redirect til Dashboard efter login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";

    // VIKTIGT: Efter login gå til Dashboard
    options.Events.OnSignedIn = async context =>
    {
        // Hvis der ikke er angivet en returnUrl, gå til Dashboard
        if (string.IsNullOrEmpty(context.Properties.RedirectUri))
        {
            context.Properties.RedirectUri = "/Dashboard";
        }
        await Task.CompletedTask;
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("NurseOrAdmin", policy => policy.RequireRole("Nurse", "Admin"));
    options.AddPolicy("RelativeAccess", policy => policy.RequireRole("Relative", "Nurse", "Admin"));
    options.AddPolicy("PatientAccess", policy => policy.RequireRole("Patient", "Nurse", "Admin"));
});

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISeizureService, SeizureService>();
builder.Services.AddScoped<MyAPI.Core.Interfaces.IAuthorizationService, AuthorizationService>();

// Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map MVC controllers FØR Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers();

// SEEDING (som før)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    Console.WriteLine("=== STARTER SEEDING ===");

    var roles = new[] { "Admin", "Nurse", "Relative", "Patient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Oprettede rolle: {role}");
        }
    }

    // Admin
    var adminEmail = "admin@epilepsi.dk";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            Console.WriteLine("Admin oprettet OK!");
        }
    }

    // Nurse
    var nurseEmail = "susanne@hospital.dk";
    if (await userManager.FindByEmailAsync(nurseEmail) == null)
    {
        var nurse = new ApplicationUser
        {
            UserName = nurseEmail,
            Email = nurseEmail,
            FirstName = "Susanne",
            LastName = "Jensen",
            EmailConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(nurse, "Nurse123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(nurse, "Nurse");
            Console.WriteLine("Nurse oprettet OK!");
        }
    }

    // Patienter
    Patient? patient1 = null;
    Patient? patient2 = null;

    if (!context.Patients.Any())
    {
        patient1 = new Patient
        {
            PatientId = "P001",
            CprNumber = "150385-1234",
            FirstName = "Lars",
            LastName = "Hansen",
            DateOfBirth = new DateTime(1985, 3, 15),
            Gender = Gender.Male,
            Diagnosis = "Epilepsi med fokale anfald",
            Notes = "Patient siden 2020, god effekt af medicin",
            CreatedAt = DateTime.UtcNow
        };

        patient2 = new Patient
        {
            PatientId = "P002",
            CprNumber = "220792-5678",
            FirstName = "Mette",
            LastName = "Nielsen",
            DateOfBirth = new DateTime(1992, 7, 22),
            Gender = Gender.Female,
            Diagnosis = "Generaliseret epilepsi",
            Notes = "Hyppige anfald, under observation",
            CreatedAt = DateTime.UtcNow
        };

        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();
        Console.WriteLine("Patienter oprettet: P001 og P002");
    }
    else
    {
        patient1 = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == "P001");
        patient2 = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == "P002");
    }

    // Relative til Lars
    var relativeEmail = "mor@familie.dk";
    if (await userManager.FindByEmailAsync(relativeEmail) == null && patient1 != null)
    {
        var relative = new ApplicationUser
        {
            UserName = relativeEmail,
            Email = relativeEmail,
            FirstName = "Karen",
            LastName = "Hansen",
            AssignedPatientId = patient1.Id,
            EmailConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(relative, "Relative123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(relative, "Relative");
            Console.WriteLine($"Relative '{relativeEmail}' oprettet");
        }
    }

    // Patient-bruger til Mette
    var patientUserEmail = "mette@patient.dk";
    if (await userManager.FindByEmailAsync(patientUserEmail) == null && patient2 != null)
    {
        var patientUser = new ApplicationUser
        {
            UserName = patientUserEmail,
            Email = patientUserEmail,
            FirstName = "Mette",
            LastName = "Nielsen",
            AssignedPatientId = patient2.Id,
            EmailConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(patientUser, "Patient123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(patientUser, "Patient");
            Console.WriteLine($"Patient '{patientUserEmail}' oprettet");
        }
    }

    // Relative til Mette
    var relative2Email = "far@familie.dk";
    if (await userManager.FindByEmailAsync(relative2Email) == null && patient2 != null)
    {
        var relative2 = new ApplicationUser
        {
            UserName = relative2Email,
            Email = relative2Email,
            FirstName = "Peter",
            LastName = "Nielsen",
            AssignedPatientId = patient2.Id,
            EmailConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(relative2, "Relative123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(relative2, "Relative");
            Console.WriteLine($"Relative '{relative2Email}' oprettet");
        }
    }

    try
    {
        await DbSeeder.SeedDataAsync(context, userManager);
        Console.WriteLine("Database seeding gennemført.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fejl under seeding: {ex.Message}");
    }

    Console.WriteLine("------------------------------------------------");
    Console.WriteLine("Login information:");
    Console.WriteLine("Admin: admin@epilepsi.dk / Admin123!");
    Console.WriteLine("Nurse: susanne@hospital.dk / Nurse123!");
    Console.WriteLine("Relative (Lars): mor@familie.dk / Relative123!");
    Console.WriteLine("Relative (Mette): far@familie.dk / Relative123!");
    Console.WriteLine("Patient (Mette): mette@patient.dk / Patient123!");
    Console.WriteLine("------------------------------------------------");
}

app.Run();