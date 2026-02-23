using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages.Seizures;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public int SeizureId { get; set; }

    [BindProperty]
    public SeizureType SeizureType { get; set; }

    [BindProperty]
    public List<int> SelectedSymptomIds { get; set; } = new();

    [BindProperty]
    public bool ConsciousnessLoss { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    public string PatientName { get; set; } = "";
    public int PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string RegisteredByName { get; set; } = "";
    public List<SelectListItem> SymptomList { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        var seizure = await _context.Seizures
            .Include(s => s.Patient)
            .Include(s => s.SeizureSymptoms)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (seizure == null) return NotFound();

        // Tjek adgang - alle må edit, men Relative/Patient kun deres egen patient
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        // Load data
        SeizureId = seizure.Id;
        SeizureType = seizure.Type;
        ConsciousnessLoss = seizure.ConsciousnessLoss;
        Notes = seizure.Notes;
        PatientId = seizure.PatientId;
        PatientName = $"{seizure.Patient.FirstName} {seizure.Patient.LastName}";
        StartTime = seizure.StartTime;
        EndTime = seizure.EndTime;
        RegisteredByName = seizure.RegisteredByName ?? "Ukendt";
        SelectedSymptomIds = seizure.SeizureSymptoms.Select(ss => ss.SymptomId).ToList();

        // Load symptoms for dropdown
        var symptoms = await _context.Symptoms.Where(s => !s.IsDeleted).ToListAsync();
        SymptomList = symptoms.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name,
            Selected = SelectedSymptomIds.Contains(s.Id)
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        var seizure = await _context.Seizures
            .Include(s => s.SeizureSymptoms)
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

        // Opdater felter
        seizure.Type = SeizureType;
        seizure.ConsciousnessLoss = ConsciousnessLoss;
        seizure.Notes = Notes;
        seizure.UpdatedBy = user.Id;
        seizure.UpdatedAt = DateTime.UtcNow;

        // Opdater symptomer - fjern gamle, tilføj nye
        var currentSymptomIds = seizure.SeizureSymptoms.Select(ss => ss.SymptomId).ToList();

        // Fjern ikke-valgte symptomer
        var symptomsToRemove = seizure.SeizureSymptoms
            .Where(ss => !SelectedSymptomIds.Contains(ss.SymptomId))
            .ToList();
        foreach (var ss in symptomsToRemove)
        {
            seizure.SeizureSymptoms.Remove(ss);
        }

        // Tilføj nye symptomer
        foreach (var symptomId in SelectedSymptomIds)
        {
            if (!seizure.SeizureSymptoms.Any(ss => ss.SymptomId == symptomId))
            {
                seizure.SeizureSymptoms.Add(new SeizureSymptom
                {
                    SymptomId = symptomId,
                    SeizureId = seizure.Id
                });
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = seizure.PatientId });
    }
}