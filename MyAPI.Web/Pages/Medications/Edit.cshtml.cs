using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Medications;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public EditModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public MedicationInputModel MedicationInput { get; set; } = new();

    public string PatientName { get; set; } = string.Empty;

    public class MedicationInputModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PrescribedBy { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var medication = await _unitOfWork.Medications.GetByIdAsync(id);
        if (medication == null)
        {
            return NotFound();
        }

        var patient = await _unitOfWork.Patients.GetByIdAsync(medication.PatientId);
        if (patient == null)
        {
            return NotFound();
        }

        PatientName = $"{patient.FirstName} {patient.LastName}";

        MedicationInput = new MedicationInputModel
        {
            Id = medication.Id,
            PatientId = medication.PatientId,
            Name = medication.Name,
            Dosage = medication.Dosage,
            Frequency = medication.Frequency,
            StartDate = medication.StartDate,
            EndDate = medication.EndDate,
            PrescribedBy = medication.PrescribedBy
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(MedicationInput.PatientId);
            if (patient != null)
            {
                PatientName = $"{patient.FirstName} {patient.LastName}";
            }
            return Page();
        }

        var medication = await _unitOfWork.Medications.GetByIdAsync(MedicationInput.Id);
        if (medication == null)
        {
            return NotFound();
        }

        medication.Name = MedicationInput.Name;
        medication.Dosage = MedicationInput.Dosage;
        medication.Frequency = MedicationInput.Frequency;
        medication.StartDate = MedicationInput.StartDate;
        medication.EndDate = MedicationInput.EndDate;
        medication.PrescribedBy = MedicationInput.PrescribedBy;
        medication.UpdatedAt = DateTime.UtcNow;
        medication.UpdatedBy = User.Identity?.Name ?? "System";

        // Brug Update (ikke UpdateAsync) - det er en synkron metode
        _unitOfWork.Medications.Update(medication);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = medication.PatientId });
    }
}