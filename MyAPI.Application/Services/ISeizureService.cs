using MyAPI.Application.DTOs;

namespace MyAPI.Application.Services;

public interface ISeizureService
{
    Task<SeizureDto> StartSeizureAsync(StartSeizureDto dto, string userId, string userName);
    Task<SeizureDto> StopSeizureAsync(StopSeizureDto dto, string userId);
    Task<IEnumerable<SeizureDto>> GetPatientSeizuresAsync(int patientId);
    Task<SeizureStatisticsDto> GetPatientStatisticsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
}