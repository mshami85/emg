using Emergency.Classes;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using X.PagedList;

namespace Emergency.Controllers
{
    [Authorize(Roles = UserRoles.WEB_USER)]
    public class MobileController : Controller
    {
        private readonly DBContext _context;

        public MobileController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page = null, bool? unread = null, MobileStatus? status = null, string? text = null, DateTime? date = null)
        {
            this.LoadState();
            text = text?.Ar2En();
            try
            {
                var messages = await _context.MobileMessages.Include(mm => mm.Mobile)
                                                            .Where(m => date == null || m.SendTime.Date == date.Value.Date)
                                                            .Where(m => string.IsNullOrEmpty(text) || m.Mobile.Owner.Contains(text) || (m.Text != null && m.Text.Contains(text)))
                                                            .Where(m => status == null || m.Status == status.Value)
                                                            .Where(m => unread == null || m.Shown == false)
                                                            .OrderByDescending(m => m.Id)
                                                            .ToPagedListAsync(page ?? 1, 50);

                //foreach (var msg in messages)
                //{
                //    msg.Shown = true;
                //    _context.MobileMessages.Update(msg);
                //}
                var latest_statues = await LatestStatuesAsync();

                //await _context.SaveChangesAsync();
                return View(new MessageIndexViewModel
                {
                    AllMessages = messages,
                    LatestStatues = latest_statues
                });
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetLatestStatues()
        {
            try
            {
                var status = await LatestStatuesAsync();
                var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles });
                return Json(json);
            }
            catch (Exception ex)
            {
                return Json("[]");
            }
        }

        async Task<IEnumerable<MobileMessage>> LatestStatuesAsync()
        {
            return await _context.MobileMessages.Include(m => m.Mobile)
                                                .Where(mm => mm.Id == _context.MobileMessages.Where(m => m.Mobile.Id == mm.Mobile.Id)
                                                                                             .Max(m => m.Id))
                                                .AsNoTracking()
                                                .ToListAsync();
        }

        [HttpGet]
        public async Task<ActionResult> Message(int? id)
        {
            if (id == null)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Index");
            }
            try
            {
                var message = await _context.MobileMessages.Include(mm => mm.Mobile)
                                                           .SingleOrDefaultAsync(mm => mm.Id == id);
                if (message == null)
                {
                    this.SetState(JobStatus.FAIL, "Message not found");
                    return RedirectToAction("Index");
                }
                if (!message.Shown)
                {
                    message.Shown = true;
                    _context.MobileMessages.Update(message);
                    await _context.SaveChangesAsync();
                }

                return View(message);
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetEventCounter()
        {
            var counter = await _context.MobileMessages.Where(mm => !mm.Shown).CountAsync();
            return Json(counter);
        }
    }
}
