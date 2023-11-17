using Emergency.Data;
using Emergency.Extensions;
using Emergency.Models;
using Emergency.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Emergency.Hubs
{
    public class NotifyActions
    {
        public const string NotifyWeb = "NotifyWeb";
        public const string NotifyChat = "NotifyChat";
        public const string NotifyMobile = "NotifyMobile";
    }

    [Authorize]
    public class MessageHub : Hub
    {
        private readonly DBContext _context;
        IConnectedUserService _userService;

        public MessageHub(DBContext context, IConnectedUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var id = Context.User.GetId();
                var name = Context.User.GetName();
                var role = Context.User.GetRole();
                _userService.Add(Context.ConnectionId, id, name, role);
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }
            catch
            {
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _userService.Remove(Context.ConnectionId);
            }
            catch
            {
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Chat(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            try
            {
                var (Id, Name, Role) = _userService.Get(Context.ConnectionId);
                var msg = new ChatMessage
                {
                    Message = message,
                    Sender = Name,
                    SendTime = DateTime.Now
                };
                await _context.ChatMessages.AddAsync(msg);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync(NotifyActions.NotifyChat, msg);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
