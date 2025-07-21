using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace Moodle_Migration_WebUI.Hubs
{
    [AllowAnonymous]
    public class StatusHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var username = Context.UserIdentifier;
            return base.OnConnectedAsync();
        }
        public async Task SendStatus(string message)
        {
            await Clients.All.SendAsync("ReceiveStatus", message);
        }
    }
}
