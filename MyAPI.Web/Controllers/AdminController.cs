using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPI.Infrastructure.Entities;
using MyAPI.Web.Models;

namespace MyAPI.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userList = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = string.Join(", ", roles), // Henter fra Identity roles
                AssignedPatientId = user.AssignedPatientId,
                CreatedAt = user.CreatedAt
            });
        }

        return View(userList);
    }

    // GET: Admin/CreateUser
    public IActionResult CreateUser()
    {
        ViewBag.Roles = new[] { "Patient", "Relative", "Nurse", "Admin" };
        return View();
    }

    // POST: Admin/CreateUser
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                AssignedPatientId = model.AssignedPatientId,
                CreatedAt = DateTime.UtcNow
                // FJERN: Role = model.Role (findes ikke længere)
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // VIKTIGT: Brug Identity roller i stedet!
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        ViewBag.Roles = new[] { "Patient", "Relative", "Nurse", "Admin" };
        return View(model);
    }

    // GET: Admin/EditUser/5
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AssignedPatientId = user.AssignedPatientId,
            Role = roles.FirstOrDefault() ?? "Patient"
        };

        ViewBag.Roles = new[] { "Patient", "Relative", "Nurse", "Admin" };
        return View(model);
    }

    // POST: Admin/EditUser/5
    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.AssignedPatientId = model.AssignedPatientId;
        // FJERN: user.Role = model.Role (findes ikke længere)

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // Opdater rolle hvis ændret
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return RedirectToAction(nameof(Users));
        }

        ViewBag.Roles = new[] { "Patient", "Relative", "Nurse", "Admin" };
        return View(model);
    }

    // POST: Admin/DeleteUser/5
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction(nameof(Users));
    }
}