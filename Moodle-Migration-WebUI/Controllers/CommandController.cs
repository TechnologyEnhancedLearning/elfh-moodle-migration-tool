using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moodle_Migration_WebUI.Hubs;
using Moodle_Migration_WebUI.Interfaces;
using Moodle_Migration_WebUI.Repositories;
using Moodle_Migration_WebUI.Services;
using System.Threading.Tasks;

namespace Moodle_Migration_WebUI.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommandService _commandService;
        private readonly IHubContext<StatusHub> _hubContext;
        private readonly LoggingDBContext _context;

        public CommandController(ICommandService commandService, IHubContext<StatusHub> hubContext, LoggingDBContext context)
        
        {
            _commandService = commandService;
            _hubContext = hubContext;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Instances = _context.MoodleInstances
                                .Where(x => x.IsActive)
                                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Execute(string command,int instanceId)
        {
            string currentUser = User.Identity?.Name;
            await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Starting...");
            // Simulate processing
            await Task.Delay(1000);
            await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Processing...");

            var result = await _commandService.ExecuteCommand(command, instanceId);
            ViewBag.Instances = _context.MoodleInstances
                            .Where(x => x.IsActive)
                            .ToList();

            ViewBag.Result = result;

            await Task.Delay(2000);
            await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Completed.");
            await Task.Delay(2000);
            return View("Index");
        }
    }
}

