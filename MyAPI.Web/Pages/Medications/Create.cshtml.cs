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

    [BindProperty]
    public Medication Medication { get; set; } = new();

    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int patientId)
    {
        PatientId = patientId;

        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
        if (patient == null)
        {
            return NotFound();
        }

        PatientName = $"{patient.FirstName} {patient.LastName}";
        Medication.PatientId = patientId;
        Medication.StartDate = DateTime.Now;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Medication.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Medications.AddAsync(Medication);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = Medication.PatientId });
    }
}