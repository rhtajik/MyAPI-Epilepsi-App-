namespace MyAPI.Application.DTOs;

public record PatientDto(
    int Id,
    string PatientId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string? Diagnosis
);

public record CreatePatientDto(
    string PatientId,
    string? CprNumber,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string? Diagnosis,
    string? Notes
);