using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Core.Interfaces;

public interface IAuthorizationService
{
    Task<bool> CanAccessPatient(string userId, int patientId);
}