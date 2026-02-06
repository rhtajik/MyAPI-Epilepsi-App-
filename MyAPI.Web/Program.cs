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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("NurseOrAdmin", policy => policy.RequireRole("Nurse", "Admin"));
    options.AddPolicy("RelativeAccess", policy => policy.RequireRole("Relative", "Nurse", "Admin"));
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISeizureService, SeizureService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapRazorPages();


// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbSeeder.SeedDataAsync(context, userManager);
}


app.Run();