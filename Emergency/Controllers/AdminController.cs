using Emergency.Classes;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Extensions;
using Emergency.Hubs;
using Emergency.Models;
using Emergency.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace Emergency.Controllers
{
    [Authorize(Roles = UserRoles.WEB_USER)]
    public class AdminController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly UserManager<User> _userManager;
        private readonly DBContext _context;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IConnectedUserService _connectedUserService;

        public AdminController(IOptionsSnapshot<AppSettings> options,
                               DBContext context,
                               UserManager<User> userManager,
                               IHubContext<MessageHub> hub,
                               IConnectedUserService connectedUserService)
        {
            _appSettings = options.Value;
            _context = context;
            _userManager = userManager;
            _hubContext = hub;
            _connectedUserService = connectedUserService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(int? msg, int? page, string? text = null, DateTime? date = null)
        {
            this.LoadState();
            text = text?.Ar2En();
            try
            {
                ViewBag.ConnectedUsers = _connectedUserService.GetConnected();
                var msgs = await _context.AdminMessages.Include(am => am.Sender)
                                                       .Include(am => am.Deliveries)
                                                       .Where(am => text == null || (am.Text != null && am.Text.Contains(text)))
                                                       .Where(am => date == null || am.SendTime.Date == date)
                                                       .AsNoTracking()
                                                       .OrderByDescending(am => am.Id)
                                                       .ToPagedListAsync(page ?? 1, 50);

                if (msg.HasValue)
                {
                    ViewBag.Message = await _context.AdminMessages.AsNoTracking()
                                                                  .Include(am => am.Deliveries)
                                                                  .ThenInclude(dl => dl.Mobile)
                                                                  .SingleOrDefaultAsync(am => am.Id == msg);
                }
                return View(msgs);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex));
            }
        }

        [HttpGet]
        public ActionResult Message()
        {
            this.LoadState();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Message(AdminMessageModel message)
        {
            if (!ModelState.IsValid)
            {
                this.SetState(JobStatus.FAIL);
                return RedirectToAction("Message");
            }
            try
            {
                var user = await _userManager.FindByNameAsync(User?.Identity?.Name);
                var admin_message = new AdminMessage
                {
                    Sender = user,
                    SendTime = DateTime.Now,
                    Text = message.Text,
                };
                admin_message.Location.LatLongFrom(message.Latitude, message.Longitude);

                var mobiles = await _context.Mobiles.Where(m => m.Enabled).ToListAsync();
                foreach (var mobile in mobiles)
                {
                    admin_message.Deliveries.Add(new MessageDelivery
                    {
                        Mobile = mobile,
                        DeliverTime = null,
                        Message = admin_message
                    });
                }
                await _context.AdminMessages.AddAsync(admin_message);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group(UserRoles.MOBILE).SendAsync(NotifyActions.NotifyMobile, false);
                this.SetState(JobStatus.SUCCESS, "يتم الآن إعلام العملاء, تمت جدولة الرسالة بنجاح");
                return RedirectToAction("Message");
            }
            catch (Exception ex)
            {
                this.SetState(JobStatus.FAIL, ex.Message);
                return RedirectToAction("Message");
            }
        }


        [HttpGet]
        public async Task<ActionResult> Chat()
        {
            var messages = await _context.ChatMessages.AsNoTracking()
                                                      .OrderByDescending(m => m.SendTime)
                                                      .Take(100)
                                                      .ToListAsync();
            messages.Reverse();
            ViewBag.UserName = User.GetName();
            ViewBag.ConnectedUsers = _connectedUserService.GetConnected();
            return View(messages);
        }

        [HttpGet]
        public async Task<ActionResult> LoadChatMessage(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var msg = await _context.ChatMessages.FindAsync(id);
            if (msg == null)
            {
                throw new NullReferenceException(nameof(msg));
            }
            var partial = msg.Sender == User.GetName() ? "_Sender" : "_Receiver";
            return PartialView(partial, msg);
        }

        [HttpGet]
        public ActionResult GetConnectedUsers()
        {
            return Json(_connectedUserService.GetConnected().Select(u => new { u.Name, u.Role }));
        }
    }
}
