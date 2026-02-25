using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Medications;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Bind kun de felter vi har brug for - IKKE Patient navigation property
    [BindProperty]
    public MedicationInputModel MedicationInput { get; set; } = new();

    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    // Separat model til form input - uden navigation properties
    public class MedicationInputModel
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PrescribedBy { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int patientId)
    {
        PatientId = patientId;

        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
        if (patient == null)
        {
            return NotFound();
        }

        PatientName = $"{patient.FirstName} {patient.LastName}";
        MedicationInput.PatientId = patientId;
        MedicationInput.StartDate = DateTime.Today;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Genindlæs patient info ved fejl
            var patient = await _unitOfWork.Patients.GetByIdAsync(MedicationInput.PatientId);
            if (patient != null)
            {
                PatientName = $"{patient.FirstName} {patient.LastName}";
                PatientId = patient.Id;
            }
            return Page();
        }

        try
        {
            // Opret Medication entity fra input
            var medication = new Medication
            {
                PatientId = MedicationInput.PatientId,
                Name = MedicationInput.Name,
                Dosage = MedicationInput.Dosage,
                Frequency = MedicationInput.Frequency,
                StartDate = MedicationInput.StartDate,
                EndDate = MedicationInput.EndDate,
                PrescribedBy = MedicationInput.PrescribedBy,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            await _unitOfWork.Medications.AddAsync(medication);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToPage("/Patients/Details", new { id = medication.PatientId });
        }
        catch (Exception ex)
        {
            // Genindlæs patient info ved fejl
            var patient = await _unitOfWork.Patients.GetByIdAsync(MedicationInput.PatientId);
            if (patient != null)
            {
                PatientName = $"{patient.FirstName} {patient.LastName}";
                PatientId = patient.Id;
            }

            ModelState.AddModelError("", $"Der opstod en fejl: {ex.Message}");
            return Page();
        }
    }
}