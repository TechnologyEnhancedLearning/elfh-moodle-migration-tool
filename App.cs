using Moodle_Migration.Interfaces;

namespace Moodle_Migration
{
    internal class App(IUserService userService, ICourseService courseService, ICategoryService categoryService, IScormService scormService)
    {
        public async Task Run(string[] args)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("* Moodle Migration from elfh Hub *");
            Console.WriteLine("----------------------------------");

            if (args.Length == 0)
            {
                while (true)
                {
                    Console.WriteLine();
                    Console.WriteLine("Enter command (or 'exit' to quit, '-h' for help): ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    string[] commandArgs = input.Split(' ');

                    if (commandArgs[0].Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    // Logic to consider search term with multiple word
                    string[] parts = input.Split('=');

                    if (parts.Length == 2)  // Ensure there's exactly one equal sign
                    {
                        string key = parts[0];   // Part before equal sign
                        string[] keyArgs = parts[0].Split(' ');
                        keyArgs[2] = keyArgs[2] + "=" + parts[1];
                        commandArgs = keyArgs;
                    }
                    await ProcessCommand(commandArgs);
                }
            }
            else
            {
                await ProcessCommand(args);
            }
        }

        private async Task ProcessCommand(string[] args)
        {
            switch (args[0])
            {
                case "-h":
                case "--help":
                    ShowHelp(args);
                    break;
                case "-u":
                case "--user":
                    await userService.ProcessUser(args);
                    break;
                case "-c":
                case "--course":
                    await courseService.ProcessCourse(args);
                    break;
                case "-ct":
                case "--category":
                    await categoryService.ProcessCategory(args);
                    break;
                case "-sc":
                case "--scorm":
                    await scormService.ProcessScorm(args);
                    break;
                default:
                    Console.WriteLine("No operation specified!");
                    break;
            }
        }

        private void ShowHelp(string[] args)
        {
            if (args.Length == 1)
            {
                ShowAllHelp();
            }
            else
            {
                switch (args[1])
                {
                    case "u":
                    case "-u":
                    case "-user":
                        ShowUserHelp();
                        break;
                    case "c":
                    case "-c":
                    case "-course":
                        ShowCourseHelp();
                        break;
                    case "ct":
                    case "-ct":
                    case "-category":
                        ShowCategoryHelp();
                        break;
                    case "sc":
                    case "-sc":
                    case "-scorm":
                        ShowScormHelp();
                        break;
                    default:
                        break;
                }
            }
        }

        private void ShowAllHelp()
        {
            Console.WriteLine("Available actions:");
            Console.WriteLine("-h, --help                               - Show help information");
            Console.WriteLine("-h, --help {option}                      - Show help information for specific option");
            Console.WriteLine("                                         - -u, --user");
            Console.WriteLine("                                         - -c, --course");
            Console.WriteLine("                                         - -ct, --category");
            Console.WriteLine("-u, --user {operation} {parameters}      - Perform user operation (details or create) with specified parameters");
            Console.WriteLine("-c, --course {operation} {parameters}    - Perform course operation (detail) with specified parameters");
            Console.WriteLine("-ct, --category {operation} {parameters} - Perform course operation (create) with specified parameters");
            Console.WriteLine("    Available operations                 - -d, --display");
            Console.WriteLine("                                         - -c, --create");
            Console.WriteLine();
            ShowUserHelp();
            ShowCourseHelp();
            ShowCategoryHelp();
            ShowScormHelp();
        }

        private void ShowCategoryHelp()
        {
            Console.WriteLine("Category actions");
            Console.WriteLine("-ct -d                                           - Display all categories without filtering");
            Console.WriteLine("-ct -d {property1}={value1} {property2}={value2} - Display categories filtering by properties");
            Console.WriteLine("-ct -c id={value}                                - Creates a top level category with the option to create child components");
            Console.WriteLine("                                                 - id={int} elfh programme component id");
            Console.WriteLine();
        }

        private void ShowCourseHelp()
        {
            Console.WriteLine("Course actions");
            Console.WriteLine("-c -d                  - Display all courses without filtering");
            Console.WriteLine("-c -d {field}={value}  - Display courses filtering by specific field");
            Console.WriteLine("                       - id={int}           course id");
            Console.WriteLine("                       - ids={int,int,int}  comma separated course ids");
            Console.WriteLine("                       - shortname={string} course short name");
            Console.WriteLine("                       - category={int}     category id the course belongs to");
            Console.WriteLine("                       - sectionid={int}    section id that belongs to a course");
            Console.WriteLine();
        }

        private void ShowUserHelp()
        {
            Console.WriteLine("User actions");
            Console.WriteLine("-u -d                                            - Display all users without filtering");
            Console.WriteLine("-u -d {property1}={value1} {property2}={value2}  - Display users filtering by properties");
            Console.WriteLine("-u -c {parameter}={value}                        - Import Users and/or Cohorts from elfh to Moodle");
            Console.WriteLine("                                                 - id={int}           import user based on elfh UserId");
            Console.WriteLine("                                                 - username={string}  import user based on elfh UserName");
            Console.WriteLine("                                                 - ugid={int}         import user group as Cohort");
            Console.WriteLine("                                                                      with option to import all users");
            Console.WriteLine();
        }
        private void ShowScormHelp()
        {
            Console.WriteLine("Scorm actions");
            Console.WriteLine("-sc -d                                           - Display all scorms without filtering");
            Console.WriteLine("-sc -d {property1}={value1} {property2}={value2} - Display categories filtering by properties");
            Console.WriteLine("-sc -c id={value}                                - Creates a top level category with the option to create child components");
            Console.WriteLine("                                                 - id={int} elfh programme component id");
            Console.WriteLine();
        }
    }
}
