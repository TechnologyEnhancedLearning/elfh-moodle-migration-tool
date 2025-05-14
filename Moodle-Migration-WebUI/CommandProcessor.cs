using System.Threading.Tasks;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Services;

namespace Moodle_Migration_WebUI
{
    public class CommandProcessor:ICommandProcessor
    {
        private readonly IUserService userService;
        private readonly ICourseService courseService;
        private readonly ICategoryService categoryService;

        public CommandProcessor(IUserService userService, ICourseService courseService, ICategoryService categoryService)
        {
            this.userService = userService;
            this.courseService = courseService;
            this.categoryService = categoryService;
        }

        public async Task<string> ProcessCommand(string[] args)
        {
            string result = string.Empty;
            if (args.Length == 0)
            {
                return "No command provided. Type '-h' for help.";
            }

            switch (args[0])
            {
                case "-h":
                case "--help":
                    result=ShowHelp(args);
                    break;
                case "-u":
                case "--user":
                    result = await userService.ProcessUser(args);                                                                              
                    break;
                case "-c":
                case "--course":
                    result = await courseService.ProcessCourse(args);
                    break;
                case "-ct":
                case "--category":
                    result = await categoryService.ProcessCategory(args);
                    break;
                default:
                    Console.WriteLine("No operation specified!");
                    break;
            }
            return result;
        }

        private string ShowHelp(string[] args)
        {
            // Simplified help logic for brevity
            return args.Length == 1
                ? "Help: Use '-u', '-c', or '-ct' with appropriate parameters."
                : $"Detailed help for command '{args[1]}' not implemented.";
        }
    }
}
