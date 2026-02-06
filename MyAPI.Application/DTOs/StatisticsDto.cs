using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Application.DTOs;

public record SeizureStatisticsDto(
    int TotalSeizures,
    double AverageDurationMinutes,
    int SeizuresThisMonth,
    Dictionary<string, int> SeizuresByType,
    List<MonthlySeizureCount> MonthlyTrend
);

public record MonthlySeizureCount(
    string Month,
    int Count,
    double AverageDurationMinutes
);