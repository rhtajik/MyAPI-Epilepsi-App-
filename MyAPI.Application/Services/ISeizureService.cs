using MyAPI.Application.DTOs;

namespace MyAPI.Application.Services;

public interface ISeizureService
{
    Task<SeizureDto> StartSeizureAsync(StartSeizureDto dto, string userId, string userName);
    Task<SeizureDto> StopSeizureAsync(StopSeizureDto dto, string userId);
    Task<IEnumerable<SeizureDto>> GetPatientSeizuresAsync(int patientId);
    Task<SeizureStatisticsDto> GetPatientStatisticsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);

    // Disse kræver ikke UserManager:
    Task<IEnumerable<SeizureDto>> GetAllSeizuresAsync();
    Task<IEnumerable<SeizureDto>> GetSeizuresByPatientIdAsync(int patientId);
    Task<SeizureDto> GetSeizureByIdAsync(int seizureId);
    // FJERN: Task<bool> CanUserAccessPatientAsync(...) - den skal være i Web laget
}