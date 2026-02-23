using Microsoft.AspNetCore.Identity;
using MyAPI.Core.Interfaces;
using MyAPI.Infrastructure.Entities;

namespace MyAPI.Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Tjek om bruger må se patient
    public async Task<bool> CanAccessPatient(string userId, int patientId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Admin & Nurse: Se alle
        if (await _userManager.IsInRoleAsync(user, "Admin") ||
            await _userManager.IsInRoleAsync(user, "Nurse"))
        {
            return true;
        }

        // Pårørende: Kun tildelt patient
        if (await _userManager.IsInRoleAsync(user, "Relative"))
        {
            return user.AssignedPatientId == patientId;
        }

        // Patient: Kun sig selv (forudsat at Patient.Id matches med AssignedPatientId 
        // eller at vi finder patienten via brugerens email/CPR)
        // BEMÆRK: Her antager vi at Patient.Id = AssignedPatientId for patient-brugere
        if (await _userManager.IsInRoleAsync(user, "Patient"))
        {
            // Patienter har deres eget ID i AssignedPatientId
            return user.AssignedPatientId == patientId;
        }

        return false;
    }

    // Tjek om bruger må registrere anfald for patient
    public async Task<bool> CanRegisterSeizure(string userId, int patientId)
    {
        // Samme logik som CanAccessPatient - alle der kan se, kan registrere
        return await CanAccessPatient(userId, patientId);
    }

    public async Task<bool> IsAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsNurseOrAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return await _userManager.IsInRoleAsync(user, "Admin") ||
               await _userManager.IsInRoleAsync(user, "Nurse");
    }
}