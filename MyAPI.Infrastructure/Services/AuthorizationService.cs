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

    public async Task<bool> CanAccessPatient(string userId, int patientId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Admin og Nurse kan se alle patienter
        if (await _userManager.IsInRoleAsync(user, "Admin") ||
            await _userManager.IsInRoleAsync(user, "Nurse"))
        {
            return true;
        }

        // Pårørende - tjek om de er tilknyttet patienten via AssignedPatientId
        if (await _userManager.IsInRoleAsync(user, "Relative"))
        {
            return user.AssignedPatientId == patientId;
        }

        // Patient - kan kun se sig selv (antager at Patient.Id matcher brugerens patient profil)
        // Hvis du har en separat Patient tabel med UserId, skal du tjekke det her
        if (await _userManager.IsInRoleAsync(user, "Patient"))
        {
            // Midlertidig løsning - patienter kan ikke se noget endnu
            // Du skal implementere logik der matcher ApplicationUser med Patient
            return false;
        }

        return false;
    }

    public async Task<bool> CanRegisterSeizure(string userId, int patientId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Admin, Nurse kan registrere for alle
        if (await _userManager.IsInRoleAsync(user, "Admin") ||
            await _userManager.IsInRoleAsync(user, "Nurse"))
        {
            return true;
        }

        // Pårørende - kun hvis tilknyttet via AssignedPatientId
        if (await _userManager.IsInRoleAsync(user, "Relative"))
        {
            return user.AssignedPatientId == patientId;
        }

        return false;
    }

    public async Task<bool> IsAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<bool> IsNurseOrAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return await _userManager.IsInRoleAsync(user, "Admin") ||
               await _userManager.IsInRoleAsync(user, "Nurse");
    }
}