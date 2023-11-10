using Emergency.Classes;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using X.PagedList;

namespace Emergency.Controllers
{
    [Authorize(Roles = UserRoles.WEB_USER)]
    public class AccountController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly DBContext _context;

        public AccountController(IOptionsSnapshot<AppSettings> options,
                                 UserManager<User> userManager,
                                 RoleManager<IdentityRole<int>> roleManager,
                                 SignInManager<User> signInManager,
                                 DBContext dBContext)
        {
            _appSettings = options.Value;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = dBContext;
        }

        [HttpGet]
        [ActionName("init-admin")]
        [AllowAnonymous]
        public async Task<ActionResult> InitAdmin()
        {
            try
            {
                var account_initilizer = new AccountInitilizer(_roleManager, _userManager);
                await account_initilizer.CreateDefaultAdmin();
                this.SetState(JobStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
            }
            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<ActionResult> Index(int? page = null)
        {
            this.LoadState();
            var users = await _context.Users.AsNoTracking()
                                            .OrderBy(u => u.UserName)
                                            .ToPagedListAsync(page ?? 1, 50);
            return View(users);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            var authenticated = _signInManager.IsSignedIn(User);
            if (authenticated)
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = "~/";
                }
                return Redirect(returnUrl);
            }

            this.LoadState();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginVM model, string? returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                this.SetState(JobStatus.FAIL, "خطأ في اسم المستخدم/كلمة المرور");
                return RedirectToAction("Login", "Account", new { @returnUrl = returnUrl });
            }

            var user = await _userManager.FindByNameAsync(model.Name.Ar2En());
            if (user == null || !user.Enabled)
            {
                this.SetState(JobStatus.FAIL, "خطأ في اسم المستخدم/كلمة المرور");
                return RedirectToAction("Login", "Account", new { @returnUrl = returnUrl });
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password.Ar2En()))
            {
                this.SetState(JobStatus.FAIL, "خطأ في اسم المستخدم/كلمة المرور");
                return RedirectToAction("Login", "Account", new { @returnUrl = returnUrl });
            }

            await _signInManager.SignInAsync(user, model.RememberMe);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "~/";
            }
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<ActionResult> SwitchState(int? id)
        {
            if (id == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            user.Enabled = !user.Enabled;
            await _userManager.UpdateAsync(user);
            this.SetState(JobStatus.SUCCESS);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> CheckUserName(string name)
        {
            var exists = await _userManager.FindByNameAsync(name.Ar2En());
            return Json(exists == null);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Create");
            }
            try
            {
                var user = new User
                {
                    UserName = model.Name.Ar2En(),
                    Enabled = true,
                    FullName = model.FullName.Ar2En()
                };

                var created = await _userManager.CreateAsync(user, model.Password.Ar2En());
                if (created.Succeeded)
                {
                    this.SetState(JobStatus.SUCCESS);
                    if (!await _roleManager.RoleExistsAsync(UserRoles.WEB_USER))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>
                        {
                            Name = UserRoles.WEB_USER
                        });
                    }
                    var roleAdded = await _userManager.AddToRoleAsync(user, UserRoles.WEB_USER);
                    if (!roleAdded.Succeeded)
                    {
                        this.SetState(JobStatus.FAIL, "Role error");
                    }
                    var claimAdded = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.FullName));
                    if (!claimAdded.Succeeded)
                    {
                        this.SetState(JobStatus.SUCCESS, "setting full name cause error");
                    }
                }
                else
                {
                    this.SetState(JobStatus.FAIL, "cannot create user");
                }
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdateUserName(int? id)
        {
            return PartialView("_UpdateUserName", id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateUserName(UpdateFullNameVM model)
        {
            if (!ModelState.IsValid)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByIdAsync(model.Id.ToString()); //db.UserProfiles.FindAsync(model.Id);
            if (user == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            user.FullName = model.FullName.Ar2En();
            var updated = await _userManager.UpdateAsync(user);
            if (updated.Succeeded)
            {
                var oldClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                if (oldClaim != null)
                {
                    await _userManager.ReplaceClaimAsync(user, oldClaim, new Claim(ClaimTypes.Surname, model.FullName));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, model.FullName));
                }
            }
            this.SetState(updated.Succeeded ? JobStatus.SUCCESS : JobStatus.FAIL);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UpdatePassword(int? id)
        {
            return PartialView("_UpdatePassword", id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePassword(int? id, string password, string confirm)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm) || password != confirm)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            var changed = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), password);
            this.SetState(changed.Succeeded ? JobStatus.SUCCESS : JobStatus.FAIL);
            return RedirectToAction("Index");
        }
    }
}