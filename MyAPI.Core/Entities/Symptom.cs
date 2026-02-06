using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Core.Entities;

public class Symptom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<SeizureSymptom> SeizureSymptoms { get; set; } = new List<SeizureSymptom>();
}