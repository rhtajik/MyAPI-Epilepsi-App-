using MyAPI.Application.DTOs;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Application.Services;

public class SeizureService : ISeizureService
{
    private readonly IUnitOfWork _unitOfWork;

    public SeizureService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SeizureDto> StartSeizureAsync(StartSeizureDto dto, string userId, string userName)
    {
        var seizure = new Seizure
        {
            PatientId = dto.PatientId,
            StartTime = DateTime.UtcNow,
            Type = dto.Type,
            ConsciousnessLoss = dto.ConsciousnessLoss,
            Notes = dto.Notes,
            RegisteredByUserId = userId,
            RegisteredByName = userName,
            CreatedBy = userId
        };

        if (dto.SymptomIds?.Count > 0)  // RETTET: .Any() -> .Count > 0
        {
            foreach (var symptomId in dto.SymptomIds)
            {
                seizure.SeizureSymptoms.Add(new SeizureSymptom { SymptomId = symptomId });
            }
        }

        await _unitOfWork.Seizures.AddAsync(seizure);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(seizure);
    }

    public async Task<SeizureDto> StopSeizureAsync(StopSeizureDto dto, string userId)
    {
        var seizure = await _unitOfWork.Seizures.GetByIdAsync(dto.SeizureId);
        if (seizure == null)
            throw new Exception($"Anfald med ID {dto.SeizureId} ikke fundet");

        if (seizure.EndTime.HasValue)
            throw new InvalidOperationException("Anfaldet er allerede stoppet");

        seizure.EndTime = DateTime.UtcNow;
        seizure.UpdatedBy = userId;

        _unitOfWork.Seizures.Update(seizure);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(seizure);
    }

    public async Task<IEnumerable<SeizureDto>> GetPatientSeizuresAsync(int patientId)
    {
        var seizures = await _unitOfWork.Seizures.FindAsync(s => s.PatientId == patientId && !s.IsDeleted);
        return seizures.Select(MapToDto);
    }

    public async Task<SeizureStatisticsDto> GetPatientStatisticsAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddYears(-1);
        toDate ??= DateTime.UtcNow;

        var seizures = await _unitOfWork.Seizures.FindAsync(s =>
            s.PatientId == patientId &&
            s.StartTime >= fromDate &&
            s.StartTime <= toDate &&
            !s.IsDeleted);

        var seizureList = seizures.ToList();

        if (seizureList.Count == 0)  // RETTET: !.Any() -> .Count == 0
        {
            return new SeizureStatisticsDto(0, 0, 0, new Dictionary<string, int>(), new List<MonthlySeizureCount>());
        }

        var durations = seizureList
            .Where(s => s.Duration.HasValue)
            .Select(s => s.Duration!.Value.TotalMinutes)
            .ToList();

        var avgDuration = durations.Count > 0 ? durations.Average() : 0;  // RETTET: .Any() -> .Count > 0

        var byType = seizureList
            .GroupBy(s => s.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var monthly = seizureList
            .GroupBy(s => new { s.StartTime.Year, s.StartTime.Month })
            .Select(g => new MonthlySeizureCount(
                $"{g.Key.Year}-{g.Key.Month:D2}",
                g.Count(),
                g.Where(s => s.Duration.HasValue).Select(s => s.Duration!.Value.TotalMinutes).DefaultIfEmpty(0).Average()
            ))
            .OrderBy(m => m.Month)
            .ToList();

        var thisMonthCount = seizureList.Count(s =>
            s.StartTime.Month == DateTime.UtcNow.Month &&
            s.StartTime.Year == DateTime.UtcNow.Year);

        return new SeizureStatisticsDto(
            seizureList.Count,
            Math.Round(avgDuration, 2),
            thisMonthCount,
            byType,
            monthly
        );
    }

    private SeizureDto MapToDto(Seizure seizure)
    {
        return new SeizureDto(
            seizure.Id,
            seizure.PatientId,
            seizure.StartTime,
            seizure.EndTime,
            seizure.Duration,
            seizure.Type.ToString(),
            seizure.ConsciousnessLoss,
            seizure.Notes,
            seizure.SeizureSymptoms?.Select(ss => ss.Symptom?.Name ?? "Unknown").ToList() ?? [],
            seizure.RegisteredByName ?? "Unknown"
        );
    }
}