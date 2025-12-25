using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StokDepo.Models;
using System;



namespace StokDepo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

    public async Task<IActionResult> Users()
{
    var users = _userManager.Users.ToList();
    var list = new List<AdminUserViewModel>();



    foreach (var u in users)
    {
        var roles = await _userManager.GetRolesAsync(u);

        list.Add(new AdminUserViewModel
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName,
            Roles = roles.ToList()
        });
    }

    return View(list);
}


 [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleAdmin(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    // Ana admin koruması (Email boşsa UserName üzerinden de kontrol)
    var seedAdminEmail = "admin@stokdepo.com";
    var email = user.Email ?? "";
    var username = user.UserName ?? "";

    if (string.Equals(email, seedAdminEmail, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(username, seedAdminEmail, StringComparison.OrdinalIgnoreCase))
    {
        TempData["Error"] = "Ana admin yetkisi kaldırılamaz.";
        return RedirectToAction(nameof(Users));
    }

    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
    if (isAdmin)
    {
        await _userManager.RemoveFromRoleAsync(user, "Admin");

        // Admin değilse User rolü kalsın
        if (!await _userManager.IsInRoleAsync(user, "User"))
            await _userManager.AddToRoleAsync(user, "User");
    }
    else
    {
        await _userManager.AddToRoleAsync(user, "Admin");
    }

    return RedirectToAction(nameof(Users));
}

    }
}
