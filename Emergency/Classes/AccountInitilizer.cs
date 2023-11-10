using Emergency.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Emergency.Classes
{
    public class AccountInitilizer
    {
        RoleManager<IdentityRole<int>> _roleManager;
        UserManager<User> _userManager;

        public AccountInitilizer(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task CreateDefaultAdmin()
        {
            if (!await _roleManager.RoleExistsAsync(UserRoles.WEB_USER))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = UserRoles.WEB_USER });
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.MOBILE))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = UserRoles.MOBILE });
            }

            var admin = await _userManager.FindByNameAsync("Admin") ?? await _userManager.FindByNameAsync("admin");

            if (admin == null)
            {
                admin = new User { UserName = "admin", Enabled = true, FullName = "Administrator" };

                var created = await _userManager.CreateAsync(admin, "admin@123");
                if (!created.Succeeded)
                {
                    throw new Exception("No users created yet");
                }
                var added = await _userManager.AddToRoleAsync(admin, UserRoles.WEB_USER);
                if (!added.Succeeded)
                {
                    throw new Exception("admin is created, but not added to role");
                }
                var claimAdded = await _userManager.AddClaimAsync(admin, new System.Security.Claims.Claim(ClaimTypes.Surname, admin.FullName));
            }

            if (!await _userManager.IsInRoleAsync(admin, UserRoles.WEB_USER))
            {
                await _userManager.AddToRoleAsync(admin, UserRoles.WEB_USER);
            }
        }
    }
}
