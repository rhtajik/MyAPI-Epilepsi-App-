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
builder.Services.AddControllersWithViews(); // MVC til AdminController

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity UDEN UI
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("NurseOrAdmin", policy => policy.RequireRole("Nurse", "Admin"));
    options.AddPolicy("RelativeAccess", policy => policy.RequireRole("Relative", "Nurse", "Admin"));
});

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// SeizureService - uden UserManager (Clean Architecture)
builder.Services.AddScoped<ISeizureService, SeizureService>();

// AuthorizationService - nu kun med UserManager (ikke IUnitOfWork)
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

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roller
    var roles = new[] { "Admin", "Nurse", "Relative", "Patient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Opret admin bruger hvis ikke eksisterer
    var adminEmail = "admin@epilepsi.dk";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Seed data
    await DbSeeder.SeedDataAsync(context, userManager);
}

app.Run();