namespace MyAPI.Core.Interfaces;

public interface IAuthorizationService
{
    Task<bool> CanAccessPatient(string userId, int patientId);
    Task<bool> CanRegisterSeizure(string userId, int patientId);
    Task<bool> IsAdmin(string userId);
    Task<bool> IsNurseOrAdmin(string userId);
}