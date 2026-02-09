namespace MyAPI.Core.Entities;

public class PatientRelative
{
    public int Id { get; set; }
    public string RelativeUserId { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? AddedByUserId { get; set; }
}