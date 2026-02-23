using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAPI.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyAPI.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";

        public int? AssignedPatientId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password skal være mindst {2} tegn langt.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords matcher ikke.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                AssignedPatientId = Input.AssignedPatientId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
                // FJERNET: Role = Input.Role (bruger Identity roller i stedet)
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // TILFØJET: Brug AddToRoleAsync i stedet for Role property
                await _userManager.AddToRoleAsync(user, Input.Role);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}