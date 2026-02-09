using Microsoft.AspNetCore.Identity;
using MyAPI.Core.Entities;

namespace MyAPI.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // KRITISK: For pårørende - hvilken patient (Id) har de adgang til?
    public int? AssignedPatientId { get; set; }

    // Fjern eller behold PatientRelatives efter behov - vi bruger AssignedPatientId i stedet
    public ICollection<PatientRelative> PatientRelatives { get; set; } = new List<PatientRelative>();
}