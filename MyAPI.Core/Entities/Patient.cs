using System.Reflection;

namespace MyAPI.Core.Entities;

public class Patient : BaseEntity
{
    public string PatientId { get; set; } = string.Empty;
    public string? CprNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }

    // Navigation properties - FJERN PatientRelatives
    public ICollection<Seizure> Seizures { get; set; } = [];
    public ICollection<Medication> Medications { get; set; } = [];
}