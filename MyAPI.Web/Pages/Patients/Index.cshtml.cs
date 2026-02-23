using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Patient> Patients { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        // Admin & Nurse: Se alle
        if (User.IsInRole("Admin") || User.IsInRole("Nurse"))
        {
            Patients = await _context.Patients
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.LastName)
                .ToListAsync();
        }
        // Pårørende: Kun tildelt patient
        else if (User.IsInRole("Relative") && user.AssignedPatientId.HasValue)
        {
            Patients = await _context.Patients
                .Where(p => p.Id == user.AssignedPatientId.Value && !p.IsDeleted)
                .ToListAsync();
        }
        // Patient: Kun sig selv (via AssignedPatientId)
        else if (User.IsInRole("Patient") && user.AssignedPatientId.HasValue)
        {
            Patients = await _context.Patients
                .Where(p => p.Id == user.AssignedPatientId.Value && !p.IsDeleted)
                .ToListAsync();
        }
    }
}