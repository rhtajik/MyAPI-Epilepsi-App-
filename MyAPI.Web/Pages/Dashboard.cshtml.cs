using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Web.Pages;

public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public DashboardModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? ErrorMessage { get; set; }

    // Properties til visning
    public List<Patient> Patients { get; set; } = new();
    public Patient? MyPatient { get; set; }
    public List<Seizure> RecentSeizures { get; set; } = new();
    public List<Medication> MyMedications { get; set; } = new();
    public Seizure? ActiveSeizure { get; set; }
    public SeizureStatisticsDto? Statistics { get; set; }

    // Roller - sættes i OnGetAsync
    public bool IsAdmin { get; set; }
    public bool IsNurse { get; set; }
    public bool IsRelative { get; set; }
    public bool IsPatient { get; set; }
    public bool IsRelativeOrPatient => IsRelative || IsPatient;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // VIKTIGT: Load roller eksplicit fra databasen
        var roles = await _userManager.GetRolesAsync(user);
        IsAdmin = roles.Contains("Admin");
        IsNurse = roles.Contains("Nurse");
        IsRelative = roles.Contains("Relative");
        IsPatient = roles.Contains("Patient");

        // Debug output til konsol
        Console.WriteLine($"=== BRUGER ROLLER ===");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Roller: {string.Join(", ", roles)}");
        Console.WriteLine($"IsAdmin: {IsAdmin}, IsNurse: {IsNurse}, IsRelative: {IsRelative}, IsPatient: {IsPatient}");
        Console.WriteLine($"=====================");

        // ==========================================
        // ADMIN & NURSE: Se alle patienter
        // ==========================================
        if (IsAdmin || IsNurse)
        {
            try
            {
                Patients = await _context.Patients
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.LastName)
                    .ToListAsync();

                RecentSeizures = await _context.Seizures
                    .AsNoTracking()
                    .Include(s => s.Patient)
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.StartTime)
                    .Take(10)
                    .ToListAsync();

                ActiveSeizure = await _context.Seizures
                    .AsNoTracking()
                    .Include(s => s.Patient)
                    .FirstOrDefaultAsync(s => !s.EndTime.HasValue && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved indlæsning af admin/nurse data: {ex.Message}");
                ErrorMessage = "Fejl ved indlæsning af data.";
            }

            return Page();
        }

        // ==========================================
        // RELATIVE & PATIENT: Kun deres tildelte patient
        // ==========================================
        else if (IsRelativeOrPatient)
        {
            if (!user.AssignedPatientId.HasValue)
            {
                ErrorMessage = "Du er ikke tilknyttet en patient. Kontakt administrator.";
                return Page();
            }

            try
            {
                MyPatient = await _context.Patients
                    .Include(p => p.Seizures.Where(s => !s.IsDeleted).OrderByDescending(s => s.StartTime))
                    .Include(p => p.Medications.Where(m => !m.IsDeleted))
                    .FirstOrDefaultAsync(p => p.Id == user.AssignedPatientId.Value && !p.IsDeleted);

                if (MyPatient == null)
                {
                    ErrorMessage = "Patient ikke fundet.";
                    return Page();
                }

                ActiveSeizure = await _context.Seizures
                    .Include(s => s.SeizureSymptoms)
                    .ThenInclude(ss => ss.Symptom)
                    .FirstOrDefaultAsync(s =>
                        s.PatientId == user.AssignedPatientId.Value &&
                        !s.EndTime.HasValue &&
                        !s.IsDeleted);

                MyMedications = await _context.Medications
                    .Where(m => m.PatientId == user.AssignedPatientId.Value && !m.IsDeleted)
                    .ToListAsync();

                await LoadStatisticsAsync(user.AssignedPatientId.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved indlæsning af relative/patient data: {ex.Message}");
                ErrorMessage = "Fejl ved indlæsning af data.";
            }

            return Page();
        }

        // Hvis ingen rolle matcher
        ErrorMessage = "Ukendt brugerrolle. Kontakt administrator.";
        return Page();
    }

    // QUICK START - For pårørende og patienter
    public async Task<IActionResult> OnPostQuickStartAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.AssignedPatientId.HasValue)
        {
            return Forbid();
        }

        // Genindlæs roller
        var roles = await _userManager.GetRolesAsync(user);
        var isRelative = roles.Contains("Relative");
        var isPatient = roles.Contains("Patient");

        if (!isRelative && !isPatient)
        {
            return Forbid();
        }

        var existingActive = await _context.Seizures
            .FirstOrDefaultAsync(s =>
                s.PatientId == user.AssignedPatientId.Value &&
                !s.EndTime.HasValue &&
                !s.IsDeleted);

        if (existingActive != null)
        {
            return RedirectToPage(new { activeSeizureId = existingActive.Id });
        }

        var seizure = new Seizure
        {
            PatientId = user.AssignedPatientId.Value,
            StartTime = DateTime.UtcNow,
            Type = SeizureType.Unknown,
            ConsciousnessLoss = false,
            Notes = "Startet hurtigt fra dashboard - detaljer udfyldes ved stop",
            RegisteredByUserId = user.Id,
            RegisteredByName = $"{user.FirstName} {user.LastName}",
            CreatedBy = user.Id
        };

        await _context.Seizures.AddAsync(seizure);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    // STOP - For alle roller
    public async Task<IActionResult> OnPostStopAsync(int seizureId, string? seizureType, string? notes, bool? consciousnessLoss)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var seizure = await _context.Seizures
            .FirstOrDefaultAsync(s => s.Id == seizureId && !s.IsDeleted);

        if (seizure == null)
        {
            ErrorMessage = "Anfald ikke fundet.";
            return RedirectToPage();
        }

        // Genindlæs roller
        var roles = await _userManager.GetRolesAsync(user);
        var isRelative = roles.Contains("Relative");
        var isPatient = roles.Contains("Patient");

        if (isRelative || isPatient)
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(seizureType) && Enum.TryParse<SeizureType>(seizureType, out var type))
            {
                seizure.Type = type;
            }
            if (!string.IsNullOrEmpty(notes))
            {
                seizure.Notes = notes;
            }
            if (consciousnessLoss.HasValue)
            {
                seizure.ConsciousnessLoss = consciousnessLoss.Value;
            }
        }

        if (seizure.EndTime.HasValue)
        {
            ErrorMessage = "Anfaldet er allerede stoppet.";
            return RedirectToPage();
        }

        seizure.EndTime = DateTime.UtcNow;
        seizure.UpdatedBy = user.Id;
        seizure.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    private async Task LoadStatisticsAsync(int patientId)
    {
        var fromDate = DateTime.UtcNow.AddYears(-1);
        var toDate = DateTime.UtcNow;

        var seizures = await _context.Seizures
            .Where(s => s.PatientId == patientId &&
                   s.StartTime >= fromDate &&
                   s.StartTime <= toDate &&
                   !s.IsDeleted)
            .ToListAsync();

        var seizureList = seizures.ToList();

        if (seizureList.Count == 0)
        {
            Statistics = new SeizureStatisticsDto(0, 0, 0, new Dictionary<string, int>(), new List<MonthlySeizureCount>());
            return;
        }

        var durations = seizureList
            .Where(s => s.Duration.HasValue)
            .Select(s => s.Duration!.Value.TotalMinutes)
            .ToList();

        var avgDuration = durations.Count > 0 ? durations.Average() : 0;

        var byType = seizureList
            .GroupBy(s => s.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var monthly = seizureList
            .GroupBy(s => new { s.StartTime.Year, s.StartTime.Month })
            .Select(g => new MonthlySeizureCount(
                $"{g.Key.Year}-{g.Key.Month:D2}",
                g.Count(),
                g.Where(s => s.Duration.HasValue).Select(s => s.Duration!.Value.TotalMinutes).DefaultIfEmpty(0).Average()
            ))
            .OrderBy(m => m.Month)
            .ToList();

        var thisMonthCount = seizureList.Count(s =>
            s.StartTime.Month == DateTime.UtcNow.Month &&
            s.StartTime.Year == DateTime.UtcNow.Year);

        Statistics = new SeizureStatisticsDto(
            seizureList.Count,
            Math.Round(avgDuration, 2),
            thisMonthCount,
            byType,
            monthly
        );
    }
}

// DTO til statistik
public record SeizureStatisticsDto(
    int TotalSeizures,
    double AverageDurationMinutes,
    int SeizuresThisMonth,
    Dictionary<string, int> SeizuresByType,
    List<MonthlySeizureCount> MonthlyTrend
);

public record MonthlySeizureCount(
    string Month,
    int Count,
    double AverageDurationMinutes
);