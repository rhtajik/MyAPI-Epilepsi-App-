using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Core.Entities;

public class Seizure : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    // KRITISK: Start/Stop funktionalitet
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

    public SeizureType Type { get; set; }
    public bool ConsciousnessLoss { get; set; }
    public string? Notes { get; set; }
    public string? RegisteredByUserId { get; set; }
    public string? RegisteredByName { get; set; } // Denormaliseret for performance

    // Many-to-many relation til symptomer
    public ICollection<SeizureSymptom> SeizureSymptoms { get; set; } = new List<SeizureSymptom>();
}

public enum SeizureType
{
    TonicClonic,        // Grand mal
    Absence,            // Petit mal
    Myoclonic,
    Atonic,
    FocalAware,         // Tidligere simple partial
    FocalImpaired,      // Tidligere complex partial
    Unknown
}