using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                user.CurrentRole = userRoles.FirstOrDefault();
                user.Roles = roles;
            }

            return View(users); // Trả về List<User>
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var oldRoles = await _userManager.GetRolesAsync(user);

            if (oldRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, oldRoles);

            await _userManager.AddToRoleAsync(user, newRole);

            user.UserType = newRole;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
    }
}