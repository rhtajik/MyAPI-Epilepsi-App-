using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Core.Entities;

public class SeizureSymptom
{
    public int SeizureId { get; set; }
    public Seizure Seizure { get; set; } = null!;
    public int SymptomId { get; set; }
    public Symptom Symptom { get; set; } = null!;
}