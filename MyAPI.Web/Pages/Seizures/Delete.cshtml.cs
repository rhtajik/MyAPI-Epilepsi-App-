using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages.Seizures;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public int SeizureId { get; set; }

    public string PatientName { get; set; } = "";
    public int PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public SeizureType SeizureType { get; set; }
    public TimeSpan? Duration { get; set; }
    public string RegisteredByName { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        var seizure = await _context.Seizures
            .Include(s => s.Patient)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (seizure == null) return NotFound();

        // Tjek adgang - alle må slette, men Relative/Patient kun deres egen patient
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        SeizureId = seizure.Id;
        PatientId = seizure.PatientId;
        PatientName = $"{seizure.Patient.FirstName} {seizure.Patient.LastName}";
        StartTime = seizure.StartTime;
        SeizureType = seizure.Type;
        Duration = seizure.Duration;
        RegisteredByName = seizure.RegisteredByName ?? "Ukendt";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        var seizure = await _context.Seizures
            .FirstOrDefaultAsync(s => s.Id == SeizureId && !s.IsDeleted);

        if (seizure == null) return NotFound();

        // Tjek adgang
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        // Soft delete
        seizure.IsDeleted = true;
        seizure.UpdatedBy = user.Id;
        seizure.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = seizure.PatientId });
    }
}