using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAPI.Core.Interfaces;

namespace MyAPI.Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    public Task<bool> CanAccessPatient(string userId, int patientId)
    {
        // Simplificeret - altid tillad for nu
        return Task.FromResult(true);
    }
}