using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Core.Entities;
using MyAPI.Core.Interfaces;

namespace MyAPI.Web.Pages.Seizures;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Seizure> Seizures { get; set; } = new List<Seizure>();

    public async Task OnGetAsync()
    {
        Seizures = await _unitOfWork.Seizures.GetAllAsync();
    }
}