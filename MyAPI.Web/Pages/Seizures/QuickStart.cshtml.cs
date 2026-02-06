using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Seizures;

public class QuickStartModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public QuickStartModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Patient> Patients { get; set; } = new List<Patient>();

    public async Task OnGetAsync()
    {
        Patients = await _unitOfWork.Patients.GetAllAsync();
    }
}