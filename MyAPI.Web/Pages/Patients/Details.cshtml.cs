using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Patients;

public class DetailsModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public DetailsModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Patient Patient { get; set; } = null!;
    public IEnumerable<Seizure> Seizures { get; set; } = [];
    public IEnumerable<Medication> Medications { get; set; } = [];
    public int Age => DateTime.Now.Year - Patient.DateOfBirth.Year;
    public int SeizureCount => Seizures.Count();
    public Seizure? LastSeizure => Seizures.OrderByDescending(s => s.StartTime).FirstOrDefault();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        Patient = patient;
        Seizures = await _unitOfWork.Seizures.FindAsync(s => s.PatientId == id && !s.IsDeleted);
        Medications = await _unitOfWork.Medications.FindAsync(m => m.PatientId == id && !m.IsDeleted);

        return Page();
    }
}