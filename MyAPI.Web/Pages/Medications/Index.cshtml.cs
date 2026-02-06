using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Medications;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Medication> Medications { get; set; } = new List<Medication>();

    public async Task OnGetAsync()
    {
        Medications = await _unitOfWork.Medications.GetAllAsync();
    }
}