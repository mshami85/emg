using Emergency.Classes;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Emergency.Controllers
{
    [Authorize(Roles = UserRoles.WEB_USER)]
    public class DeviceController : Controller
    {
        private readonly DBContext _context;

        public DeviceController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult Index(int? page)
        {
            this.LoadState();
            var mobiles = _context.Mobiles.AsNoTracking()
                                          .OrderBy(m => m.Owner)
                                          .ToPagedList(page ?? 1, 50);
            return View(mobiles);
        }

        [HttpPost]
        public ActionResult? Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            text = text.Ar2En();
            var devs = _context.Mobiles.AsNoTracking().Where(m => m.Owner.Contains(text) || m.AndroidId.Contains(text));
            return PartialView("_Search", devs);
        }

        [HttpPost]
        public ActionResult CheckAndroidId(string androidId, int? id)
        {
            androidId = androidId.Ar2En();
            var exists = _context.Mobiles.AsNoTracking().Any(m => m.AndroidId == androidId && (id == null || m.Id != id));
            return Json(!exists);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return PartialView("_CreateDevice", new DeviceModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeviceModel model)
        {
            if (!ModelState.IsValid)
            {
                this.SetState(JobStatus.FAIL);
                return View(model);
            }
            var mobile = new Mobile
            {
                AndroidId = model.AndroidId.Ar2En(),
                SecureCode = model.SecureCode.Ar2En(),
                Enabled = true,
                Notes = model.Notes,
                Owner = model.Owner.Ar2En(),
                RegisterDate = DateTime.Now
            };

            try
            {
                await _context.Mobiles.AddAsync(mobile);
                await _context.SaveChangesAsync();
                this.SetState(JobStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Modify(int? id)
        {
            if (id == null || id <= 0)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            var device = await _context.Mobiles.FindAsync(id);
            if (device == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("index");
            }

            var model = new DeviceViewModel
            {
                Id = device.Id,
                AndroidId = device.AndroidId,
                Notes = device.Notes,
                Owner = device.Owner,
                SecureCode = device.SecureCode
            };
            return PartialView("_ModifyDevice", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Modify(DeviceViewModel model)
        {
            if (!ModelState.IsValid || model.Id <= 0)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index", new { @id = model.Id });
            }

            try
            {
                var mobile = await _context.Mobiles.FindAsync(model.Id);
                if (mobile == null)
                {
                    this.SetState(JobStatus.FAIL);
                    return RedirectToAction("Index", new { @id = model.Id });
                }
                mobile.SecureCode = model.SecureCode.Ar2En();
                mobile.AndroidId = model.AndroidId.Ar2En();
                mobile.Owner = model.Owner.Ar2En();
                mobile.Notes = model.Notes;

                _context.Mobiles.Update(mobile);
                await _context.SaveChangesAsync();
                this.SetState(JobStatus.SUCCESS);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> SwitchState(int? id, int? page)
        {
            if (id == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index", new { @page = page ?? 1 });
            }
            try
            {

                var mobile = await _context.Mobiles.FindAsync(id);
                if (mobile == null)
                {
                    this.SetState(JobStatus.FAIL);
                    return RedirectToAction("Index", new { @page = page ?? 1 });
                }
                mobile.Enabled = !mobile.Enabled;
                _context.Mobiles.Update(mobile);
                await _context.SaveChangesAsync();
                this.SetState(JobStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
            }
            return RedirectToAction("Index", new { @page = page ?? 1 });
        }
    }
}