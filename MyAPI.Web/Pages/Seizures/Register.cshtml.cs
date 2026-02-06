using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Application.DTOs;
using MyAPI.Application.Services;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;
using System.Security.Claims;

namespace MyAPI.Web.Pages.Seizures;

public class RegisterModel : PageModel
{
    private readonly ISeizureService _seizureService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterModel(ISeizureService seizureService, IUnitOfWork unitOfWork)
    {
        _seizureService = seizureService;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public StartSeizureDto StartDto { get; set; } = new StartSeizureDto();

    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Seizure? ActiveSeizure { get; set; }
    public IEnumerable<Symptom> Symptoms { get; set; } = new List<Symptom>();

    public async Task<IActionResult> OnGetAsync(int patientId)
    {
        PatientId = patientId;

        var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
        if (patient == null)
        {
            return NotFound();
        }

        PatientName = $"{patient.FirstName} {patient.LastName}";

        // Tjek om der allerede er et aktivt anfald
        var seizures = await _unitOfWork.Seizures.FindAsync(s =>
            s.PatientId == patientId && !s.EndTime.HasValue && !s.IsDeleted);
        ActiveSeizure = seizures.FirstOrDefault();

        Symptoms = await _unitOfWork.Symptoms.GetAllAsync();

        // Sæt patientId i DTO
        StartDto.PatientId = patientId;

        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(int patientId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var userName = User.Identity?.Name ?? "System";

        // DTO er allerede bound fra form
        StartDto.PatientId = patientId;

        await _seizureService.StartSeizureAsync(StartDto, userId, userName);

        return RedirectToPage(new { patientId });
    }

    public async Task<IActionResult> OnPostStopAsync(int seizureId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var stopDto = new StopSeizureDto { SeizureId = seizureId };
        await _seizureService.StopSeizureAsync(stopDto, userId);

        // Hent patientId fra det stoppede anfald for redirect
        var seizure = await _unitOfWork.Seizures.GetByIdAsync(seizureId);
        return RedirectToPage("/Patients/Details", new { id = seizure?.PatientId });
    }
}