using Microsoft.AspNetCore.Identity;
using MyAPI.Core.Entities;

namespace MyAPI.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PatientRelative> PatientRelatives { get; set; } = new List<PatientRelative>();
}