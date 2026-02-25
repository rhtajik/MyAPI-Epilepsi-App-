using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyAPI.Core.Entities;
using MyAPI.Infrastructure.Data;

namespace MyAPI.Web.Pages.Seizures;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Seizure> Seizures { get; set; } = new List<Seizure>();

    public async Task OnGetAsync()
    {
        Seizures = await _context.Seizures
            .Include(s => s.Patient)  // ? Loader patient data
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }
}