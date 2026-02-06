using MyAPI.Core.Entities;

namespace MyAPI.Application.DTOs;

public record SeizureDto(
    int Id,
    int PatientId,
    DateTime StartTime,
    DateTime? EndTime,
    TimeSpan? Duration,
    string Type,
    bool ConsciousnessLoss,
    string? Notes,
    List<string> Symptoms,
    string RegisteredByName
);

public record StartSeizureDto(
    int PatientId,
    SeizureType Type,
    List<int> SymptomIds,
    bool ConsciousnessLoss,
    string? Notes
);

public record StopSeizureDto(
    int SeizureId
);