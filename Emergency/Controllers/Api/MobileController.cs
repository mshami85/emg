using Emergency.Classes;
using Emergency.Data;
using Emergency.Dtos;
using Emergency.Extensions;
using Emergency.Hubs;
using Emergency.Models;
using Emergency.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace Emergency.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.MOBILE)]
    public class MobileController : ControllerBase
    {
        AppSettings _appSettings;
        DBContext _context;
        IJwtService _jwtService;
        IHubContext<MessageHub> _hubContext;

        public MobileController(IOptionsSnapshot<AppSettings> options, DBContext context, IJwtService jwtService, IHubContext<MessageHub> hub)
        {
            _appSettings = options.Value;
            _context = context;
            _jwtService = jwtService;
            _hubContext = hub;
        }

        [HttpGet]
        public IActionResult Test()
        {
            var id = User.GetId();
            var name = User.GetName();

            return Ok(new
            {
                Id = id,
                Name = name,
                Result = "Test Ok"
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] ApiLoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ClientVersion) ||
                string.IsNullOrWhiteSpace(model.AndroidId) ||
                string.IsNullOrWhiteSpace(model.SecureCode))
            {
                return BadRequest("البيانات غير مكتملة");
            }
            try
            {
                var minVersion = _appSettings.MinVersion;
                try
                {
                    if (!Version.TryParse(model.ClientVersion, out Version? modelVersion) || modelVersion == null || modelVersion < minVersion)
                    {
                        return StatusCode(StatusCodes.Status505HttpVersionNotsupported, "التطبيق قديم جدا");
                    }
                }
                catch
                {
                    return StatusCode(StatusCodes.Status505HttpVersionNotsupported, "لا يمكن التعرف على الإصدار");
                }

                var mobile = await _context.Mobiles.SingleOrDefaultAsync(m => m.SecureCode == model.SecureCode && m.AndroidId == model.AndroidId);
                if (mobile == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "الجهاز غير معرف");
                }
                if (!mobile.Enabled)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "الجهاز غير مفعل");
                }

                return Ok(new
                {
                    Owner = mobile.Owner,
                    Token = _jwtService.GenerateToken(mobile),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("GetPendingMessages")]
        public async Task<IActionResult> GetPendingMessagesAsync()
        {
            try
            {
                var id = User.GetId();
                var device = await _context.Mobiles.FindAsync(id);
                if (device == null || !device.Enabled)
                {
                    return BadRequest("Device not found/Enabled");
                }
                var pending_messages = await _context.MessagesDelivery.Include(m => m.Message).ThenInclude(m => m.Sender)
                                                                      .Include(m => m.Mobile)
                                                                      .Where(m => m.Mobile.Id == id && m.DeliverTime == null)
                                                                      .OrderByDescending(m => m.Id)
                                                                      .ToListAsync();
                List<AdminMessageViewModel> messages = new();
                if (pending_messages.Count > 0)
                {
                    foreach (var item in pending_messages)
                    {
                        item.DeliverTime = DateTime.Now;
                        _context.MessagesDelivery.Update(item);
                        messages.Add(AdminMessageViewModel.CopyFrom(item.Message));
                    }
                    await _context.SaveChangesAsync();
                }
                return Ok(new
                {
                    Messages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("GetAllMessagesCount")]
        public async Task<IActionResult> GetAllMessagesCountAsync()
        {
            try
            {
                var id = User.GetId();
                var device = await _context.Mobiles.FindAsync(id);
                if (device == null || !device.Enabled)
                {
                    return BadRequest("Device not found/Enabled");
                }
                var messages_count = await _context.MessagesDelivery.AsNoTracking()
                                                                    .Include(md => md.Mobile)
                                                                    .Where(m => m.Mobile.Id == id)
                                                                    .CountAsync();
                return Ok(new
                {
                    Pages = (int)Math.Ceiling(messages_count / 100.0d),
                    Messages = messages_count,
                    PageMessages = 100
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("GetAllMessages")]
        public async Task<IActionResult> GetAllMessagesAsync([FromHeader] int? page = null)
        {
            try
            {
                var id = User.GetId();
                var device = await _context.Mobiles.FindAsync(id);
                if (device == null || !device.Enabled)
                {
                    return BadRequest("Device not found/Enabled");
                }

                var all_msgs = await _context.MessagesDelivery.Include(m => m.Message)
                                                              .ThenInclude(m => m.Sender)
                                                              .Include(m => m.Mobile)
                                                              .Where(m => m.Mobile.Id == id)
                                                              .OrderByDescending(m => m.Id)
                                                              .ToPagedListAsync(page ?? 1, 100);
                if (all_msgs.Count > 0)
                {
                    foreach (var item in all_msgs)
                    {
                        item.DeliverTime = DateTime.Now;
                        _context.MessagesDelivery.Update(item);
                    }
                    await _context.SaveChangesAsync();
                }

                var messages = all_msgs.Select(m => AdminMessageViewModel.CopyFrom(m.Message)).ToList();
                return Ok(new
                {
                    Messages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("Send")]
        public async Task<IActionResult> SendAsync([FromBody] MessageModel model)
        {
            if (!model.IsValid())
            {
                return BadRequest("بيانات غير مكتملة");
            }
            try
            {
                var id = User.GetId();
                var device = await _context.Mobiles.FindAsync(id);
                if (device == null || !device.Enabled)
                {
                    return BadRequest("Device not found/Enabled");
                }
                var msg = new MobileMessage
                {
                    Text = model.Message,
                    SendTime = model.SendDate,
                    Mobile = device,
                    Status = model.Status
                };
                if (model.Location.HasValue())
                {
                    msg.Location.Latitude = model.Location.Latitude;
                    msg.Location.Longitude = model.Location.Longitude;
                }

                await _context.MobileMessages.AddAsync(msg);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group(UserRoles.WEB_USER).SendAsync(NotifyActions.NotifyWeb);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("GetLatestChat")]
        public async Task<IActionResult> GetLatestChatAsync()
        {
            try
            {
                var id = User.GetId();
                var device = await _context.Mobiles.FindAsync(id);
                if (device == null || !device.Enabled)
                {
                    return BadRequest("Device not found/Enabled");
                }
                var user = User.GetName() ?? "mobile user";

                var messages = await _context.ChatMessages.AsNoTracking()
                                                          .OrderByDescending(m => m.SendTime)
                                                          .Take(100)
                                                          .ToListAsync();
                messages.Reverse();
                return Ok(new
                {
                    User = user,
                    Messages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
