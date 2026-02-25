using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Medications;

public class DeleteModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public Medication Medication { get; set; } = new();

    public string PatientName { get; set; } = string.Empty;

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

        Medication = medication;
        PatientName = $"{patient.FirstName} {patient.LastName}";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var medication = await _unitOfWork.Medications.GetByIdAsync(id);
        if (medication == null)
        {
            return NotFound();
        }

        var patientId = medication.PatientId;

        // Brug SoftDelete (ikke DeleteAsync) - det er en synkron metode
        // Dette sætter typisk IsDeleted = true i stedet for at fjerne fra databasen
        _unitOfWork.Medications.SoftDelete(medication);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = patientId });
    }
}