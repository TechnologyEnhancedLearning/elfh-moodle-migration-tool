using Moodle_Migration;
using Moodle_Migration.Interfaces;
using Moodle_Migration_WebUI.Interfaces;
using System.Threading.Tasks;

namespace Moodle_Migration_WebUI.Services
{
    public class CommandService : ICommandService
    {
        private readonly ICommandProcessor _commandProcessor;
        public CommandService(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        public async Task<string> ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "Command cannot be empty.";
            }

            // Split input into arguments
            string[] args = input.Split(' ');

            // Call the CommandProcessor
            return  await _commandProcessor.ProcessCommand(args);

            // Simulate command processing logic
            //string[] args = input.Split(' ');
            //return Task.FromResult(args[0] switch
            //{
            //    "-h" or "--help" => "Help: Use '-u' for user, '-c' for course, and '-ct' for category commands.",
            //    "-u" or "--user" => "Processing user command...",
            //    "-c" or "--course" => "Processing course command...",
            //    "-ct" or "--category" => "Processing category command...",
            //    _ => "Unknown command! Type '-h' for help."
            //});
        }
    }
}
