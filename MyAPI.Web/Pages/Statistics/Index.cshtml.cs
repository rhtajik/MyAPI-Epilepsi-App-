using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Statistics;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public int TotalPatients { get; set; }
    public int TotalSeizures { get; set; }
    public int ActiveSeizures { get; set; }
    public Dictionary<string, int> SeizuresByType { get; set; } = new();

    public async Task OnGetAsync()
    {
        var patients = await _unitOfWork.Patients.GetAllAsync();
        var seizures = await _unitOfWork.Seizures.GetAllAsync();

        TotalPatients = patients.Count();
        TotalSeizures = seizures.Count();
        ActiveSeizures = seizures.Count(s => !s.EndTime.HasValue);

        SeizuresByType = seizures
            .GroupBy(s => s.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }
}