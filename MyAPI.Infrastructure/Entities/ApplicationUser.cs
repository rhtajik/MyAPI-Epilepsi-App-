using Microsoft.AspNetCore.Identity;
using MyAPI.Core.Entities;

namespace MyAPI.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedPatientId { get; set; }


    [Obsolete("Brug ikke denne - brug Identity roller i stedet")]
    public UserRole Role { get; set; }


}