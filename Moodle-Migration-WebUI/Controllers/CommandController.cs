using Microsoft.AspNetCore.Mvc;
using Moodle_Migration_WebUI.Interfaces;
using Moodle_Migration_WebUI.Services;
using System.Threading.Tasks;

namespace Moodle_Migration_WebUI.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommandService _commandService;

        public CommandController(ICommandService commandService)
        
        {
            _commandService = commandService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Execute(string command)
        {
            var result = await _commandService.ExecuteCommand(command);
            ViewBag.Result = result;
            return View("Index");
        }
    }
}

