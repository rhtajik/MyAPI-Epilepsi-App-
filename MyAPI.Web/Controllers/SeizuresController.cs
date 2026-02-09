using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Application.DTOs;
using MyAPI.Application.Services;
using MyAPI.Infrastructure.Entities;
using System.Security.Claims;

namespace MyAPI.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeizuresController : ControllerBase
{
    private readonly ISeizureService _seizureService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeizuresController(
        ISeizureService seizureService,
        UserManager<ApplicationUser> userManager)
    {
        _seizureService = seizureService;
        _userManager = userManager;
    }

    // GET: api/seizures/my-seizures - Hovedendpoint til at hente anfald baseret på rolle
    [HttpGet("my-seizures")]
    public async Task<ActionResult<IEnumerable<SeizureDto>>> GetMySeizures()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // ADMIN: Se alle anfald
        if (User.IsInRole("Admin"))
        {
            var allSeizures = await _seizureService.GetAllSeizuresAsync();
            return Ok(allSeizures);
        }

        // NURSE: Se alle anfald
        if (User.IsInRole("Nurse"))
        {
            var allSeizures = await _seizureService.GetAllSeizuresAsync();
            return Ok(allSeizures);
        }

        // PÅRØRENDE: Kun deres tildelte patients anfald
        if (User.IsInRole("Relative"))
        {
            if (!user.AssignedPatientId.HasValue)
            {
                return BadRequest(new { message = "Du er ikke tilknyttet nogen patient. Kontakt administrator." });
            }

            var seizures = await _seizureService.GetSeizuresByPatientIdAsync(user.AssignedPatientId.Value);
            return Ok(seizures);
        }

        // PATIENT: Her skal du implementere logikken hvis patienter selv skal se deres anfald
        // Det kræver at Patient.Id er knyttet til ApplicationUser.Id på en eller anden måde
        if (User.IsInRole("Patient"))
        {
            // Hvis Patient tabellen har et UserId felt der matcher:
            // var patient = await _patientService.GetByUserIdAsync(user.Id);
            // var seizures = await _seizureService.GetSeizuresByPatientIdAsync(patient.Id);

            // Midlertidig løsning - returner tom liste eller fejl
            return Ok(new List<SeizureDto>());
        }

        return Forbid();
    }

    [HttpPost("start")]
    [Authorize(Roles = "Nurse,Admin,Relative")]
    public async Task<ActionResult<SeizureDto>> StartSeizure([FromBody] StartSeizureDto dto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // ADGANGSKONTROL: Hvis pårørende, tjek at de har adgang til denne patient
            if (User.IsInRole("Relative"))
            {
                if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != dto.PatientId)
                {
                    return Forbid();
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? user.FirstName;

            var result = await _seizureService.StartSeizureAsync(dto, userId, userName);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("stop")]
    [Authorize(Roles = "Nurse,Admin,Relative")]
    public async Task<ActionResult<SeizureDto>> StopSeizure([FromBody] StopSeizureDto dto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Hent anfaldet for at tjekke adgang
            var seizure = await _seizureService.GetSeizureByIdAsync(dto.SeizureId);
            if (seizure == null)
                return NotFound(new { message = "Anfald ikke fundet" });

            // Pårørende tjek
            if (User.IsInRole("Relative"))
            {
                if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != seizure.PatientId)
                {
                    return Forbid();
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _seizureService.StopSeizureAsync(dto, userId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("patient/{patientId}/history")]
    [Authorize(Roles = "Nurse,Admin,Relative")]
    public async Task<ActionResult<IEnumerable<SeizureDto>>> GetPatientHistory(int patientId)
    {
        var user = await _userManager.GetUserAsync(User);

        // Pårørende tjek
        if (User.IsInRole("Relative"))
        {
            if (!user.AssignedPatientId.HasValue || user.AssignedPatientId.Value != patientId)
            {
                return Forbid();
            }
        }

        var seizures = await _seizureService.GetSeizuresByPatientIdAsync(patientId);
        return Ok(seizures);
    }

    [HttpGet("patient/{patientId}/statistics")]
    [Authorize(Roles = "Nurse,Admin")]
    public async Task<ActionResult<SeizureStatisticsDto>> GetStatistics(int patientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _seizureService.GetPatientStatisticsAsync(patientId, from, to);
        return Ok(stats);
    }

    // Tilføj denne hvis du vil kunne hente specifikke anfald med adgangskontrol
    [HttpGet("{id}")]
    public async Task<ActionResult<SeizureDto>> GetSeizure(int id)
    {
        var seizure = await _seizureService.GetSeizureByIdAsync(id);
        if (seizure == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);

        // Tjek adgang
        if (User.IsInRole("Relative") && user.AssignedPatientId != seizure.PatientId)
            return Forbid();

        if (User.IsInRole("Patient"))
        {
            // Implementer patient logik her
            return Forbid();
        }

        return Ok(seizure);
    }
}