using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages.Seizures;

public class QuickStartModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuickStartModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Patient> Patients { get; set; } = new();
    public bool IsSinglePatient { get; set; } = false;
    public int? SinglePatientId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        // Hvis pårørende eller patient: De har kun én patient
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (user.AssignedPatientId.HasValue)
            {
                // Hop direkte til registreringssiden med det samme!
                return RedirectToPage("/Seizures/Register", new { patientId = user.AssignedPatientId.Value });
            }
            else
            {
                // Fejl - pårørende uden tildelt patient
                ModelState.AddModelError("", "Du er ikke tilknyttet en patient. Kontakt administrator.");
                return Page();
            }
        }

        // Hvis Nurse/Admin: Vis liste over alle patienter
        Patients = await _context.Patients
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.LastName)
            .ToListAsync();

        return Page();
    }
}