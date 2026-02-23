using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Application.DTOs;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages.Seizures;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int PatientId { get; set; }

    public string PatientName { get; set; } = "";

    [BindProperty]
    public StartSeizureDto StartDto { get; set; } = new();

    [BindProperty]
    public StopSeizureDto StopDto { get; set; } = new();

    public Seizure? ActiveSeizure { get; set; }
    public List<Symptom> Symptoms { get; set; } = new();

    // NY: Til at vise formularen efter stop
    public bool ShowSaveForm { get; set; } = false;

    public async Task<IActionResult> OnGetAsync(int patientId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        PatientId = patientId;

        // Tjek adgang baseret på rolle
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (user.AssignedPatientId != patientId)
            {
                return Forbid();
            }
        }
        else if (!User.IsInRole("Nurse") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Hent patient navn
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null) return NotFound();
        PatientName = $"{patient.FirstName} {patient.LastName}";

        // Tjek om der allerede er et aktivt anfald for denne patient
        ActiveSeizure = await _context.Seizures
            .Include(s => s.SeizureSymptoms)
            .ThenInclude(ss => ss.Symptom)
            .FirstOrDefaultAsync(s => s.PatientId == patientId && !s.EndTime.HasValue && !s.IsDeleted);

        // Hent symptomer til dropdown
        Symptoms = await _context.Symptoms.Where(s => !s.IsDeleted).ToListAsync();

        return Page();
    }

    // QUICK START - For alle roller (start med det samme!)
    public async Task<IActionResult> OnPostQuickStartAsync(int patientId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Tjek adgang
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (user.AssignedPatientId != patientId)
                return Forbid();
        }
        else if (!User.IsInRole("Nurse") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Tjek om der allerede er et aktivt anfald
        var existingActive = await _context.Seizures
            .FirstOrDefaultAsync(s => s.PatientId == patientId && !s.EndTime.HasValue && !s.IsDeleted);

        if (existingActive != null)
        {
            return RedirectToPage(new { patientId = patientId });
        }

        // Start anfald med det samme
        var seizure = new Seizure
        {
            PatientId = patientId,
            StartTime = DateTime.UtcNow,
            Type = SeizureType.Unknown,
            ConsciousnessLoss = false,
            Notes = "Startet hurtigt - detaljer udfyldes ved stop",
            RegisteredByUserId = user.Id,
            RegisteredByName = $"{user.FirstName} {user.LastName}",
            CreatedBy = user.Id
        };

        await _context.Seizures.AddAsync(seizure);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { patientId = patientId });
    }

    // STOP - Stopper tidtagning og viser formularen (2-trins proces)
    public async Task<IActionResult> OnPostStopAsync(int seizureId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var seizure = await _context.Seizures
            .Include(s => s.Patient)
            .FirstOrDefaultAsync(s => s.Id == seizureId && !s.IsDeleted);

        if (seizure == null)
        {
            ModelState.AddModelError("", "Anfald ikke fundet.");
            return RedirectToPage(new { patientId = 0 });
        }

        // Tjek adgang
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        if (seizure.EndTime.HasValue)
        {
            ModelState.AddModelError("", "Anfaldet er allerede stoppet.");
            return RedirectToPage(new { patientId = seizure.PatientId });
        }

        // Stop tidtagning men gem IKKE endeligt endnu
        seizure.EndTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Vis formularen for at udfylde detaljer
        ShowSaveForm = true;
        ActiveSeizure = seizure;
        PatientId = seizure.PatientId;
        PatientName = $"{seizure.Patient.FirstName} {seizure.Patient.LastName}";
        Symptoms = await _context.Symptoms.Where(s => !s.IsDeleted).ToListAsync();

        return Page();
    }

    // SAVE - Gemmer detaljerne (2. trin)
    public async Task<IActionResult> OnPostSaveAsync(
        int seizureId,
        string seizureType,
        string? notes,
        bool? consciousnessLoss,
        List<int> symptomIds)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var seizure = await _context.Seizures
            .Include(s => s.SeizureSymptoms)
            .Include(s => s.Patient)
            .FirstOrDefaultAsync(s => s.Id == seizureId && !s.IsDeleted);

        if (seizure == null)
        {
            ModelState.AddModelError("", "Anfald ikke fundet.");
            return RedirectToPage(new { patientId = 0 });
        }

        // Tjek adgang
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        // VALIDÉR: Anfaldstype skal vælges
        if (string.IsNullOrEmpty(seizureType))
        {
            ModelState.AddModelError("", "Anfaldstype skal vælges.");
            ShowSaveForm = true;
            ActiveSeizure = seizure;
            PatientId = seizure.PatientId;
            PatientName = $"{seizure.Patient.FirstName} {seizure.Patient.LastName}";
            Symptoms = await _context.Symptoms.Where(s => !s.IsDeleted).ToListAsync();
            return Page();
        }

        // Opdater detaljer
        if (Enum.TryParse<SeizureType>(seizureType, out var type))
        {
            seizure.Type = type;
        }

        seizure.ConsciousnessLoss = consciousnessLoss ?? false;
        seizure.Notes = notes;
        seizure.UpdatedBy = user.Id;
        seizure.UpdatedAt = DateTime.UtcNow;

        // Tilføj symptomer
        if (symptomIds != null && symptomIds.Any())
        {
            foreach (var symptomId in symptomIds)
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
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = seizure.PatientId });
    }

    // CANCEL - Annullerer stop (fortsætter anfaldet)
    public async Task<IActionResult> OnPostCancelAsync(int seizureId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var seizure = await _context.Seizures
            .FirstOrDefaultAsync(s => s.Id == seizureId && !s.IsDeleted);

        if (seizure == null)
        {
            return RedirectToPage(new { patientId = 0 });
        }

        // Tjek adgang
        if (User.IsInRole("Relative") || User.IsInRole("Patient"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }
        }

        // Fjern EndTime for at fortsætte anfaldet
        seizure.EndTime = null;
        await _context.SaveChangesAsync();

        return RedirectToPage(new { patientId = seizure.PatientId });
    }
}