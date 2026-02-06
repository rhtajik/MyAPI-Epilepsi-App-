using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Application.DTOs;
using MyAPI.Application.Services;
using System.Security.Claims;

namespace MyAPI.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeizuresController : ControllerBase
{
    private readonly ISeizureService _seizureService;

    public SeizuresController(ISeizureService seizureService)
    {
        _seizureService = seizureService;
    }

    [HttpPost("start")]
    [Authorize(Roles = "Nurse,Admin,Relative")]
    public async Task<ActionResult<SeizureDto>> StartSeizure([FromBody] StartSeizureDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userName = User.FindFirstValue(ClaimTypes.Name)!;

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

    [HttpGet("patient/{patientId}/statistics")]
    [Authorize(Roles = "Nurse,Admin")]
    public async Task<ActionResult<SeizureStatisticsDto>> GetStatistics(int patientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _seizureService.GetPatientStatisticsAsync(patientId, from, to);
        return Ok(stats);
    }
}