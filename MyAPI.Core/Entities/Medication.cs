using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAPI.Core.Entities;

public class Medication : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty; // f.eks. "500mg"
    public string Frequency { get; set; } = string.Empty; // f.eks. "2 gange dagligt"
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PrescribedBy { get; set; }
    public bool IsActive => EndDate == null || EndDate > DateTime.Now;
}