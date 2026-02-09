using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;
using System.Security.Claims;

namespace MyAPI.Web.Pages.Patients;

[Authorize(Roles = "Admin,Nurse")] // Kun Admin og Nurse kan se alle patienter
public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IList<Patient> Patients { get; set; } = new List<Patient>();

    public async Task OnGetAsync()
    {
        // Hent alle patienter fra databasen
        var patients = await _unitOfWork.Patients.GetAllAsync();
        Patients = patients.ToList();
    }
}