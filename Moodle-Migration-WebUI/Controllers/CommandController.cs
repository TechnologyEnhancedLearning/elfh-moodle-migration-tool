using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moodle_Migration_WebUI.Hubs;
using Moodle_Migration_WebUI.Interfaces;
using Moodle_Migration_WebUI.Services;
using System.Threading.Tasks;

namespace Moodle_Migration_WebUI.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommandService _commandService;
        private readonly IHubContext<StatusHub> _hubContext;

        public CommandController(ICommandService commandService, IHubContext<StatusHub> hubContext)
        
        {
            _commandService = commandService;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Execute(string command)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", "Starting...");
            // Simulate processing
            await Task.Delay(1000);
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", "Processing...");

            var result = await _commandService.ExecuteCommand(command);
            ViewBag.Result = result;
            await Task.Delay(2000);
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", "Completed.");
            await Task.Delay(2000);
            return View("Index");
        }
    }
}

