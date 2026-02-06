using MyAPI.Core.Entities;

namespace MyAPI.Application.DTOs;

// START ANFALD DTO - class med setters
public class StartSeizureDto
{
    public int PatientId { get; set; }
    public SeizureType Type { get; set; }
    public List<int> SymptomIds { get; set; } = new List<int>();
    public bool ConsciousnessLoss { get; set; }
    public string? Notes { get; set; }
}

// STOP ANFALD DTO - class med setter
public class StopSeizureDto
{
    public int SeizureId { get; set; }
}

// SEIZURE DTO - record til visning
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

// STATISTIK DTO - record
public record SeizureStatisticsDto(
    int TotalSeizures,
    double AverageDurationMinutes,
    int SeizuresThisMonth,
    Dictionary<string, int> SeizuresByType,
    List<MonthlySeizureCount> MonthlyTrend
);

// MÅNEDLIG TÆLLING - record
public record MonthlySeizureCount(
    string Month,
    int Count,
    double AverageDurationMinutes
);