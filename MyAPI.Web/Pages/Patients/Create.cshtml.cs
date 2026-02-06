using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public Patient Patient { get; set; } = new Patient();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Patient.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Patients.AddAsync(Patient);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage("/Patients/Index");
    }
}