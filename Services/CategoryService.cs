using Microsoft.SqlServer.Server;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Security.Cryptography.X509Certificates;

namespace Moodle_Migration.Services
{
    public class CategoryService(IHttpService httpService, IComponentRepository componentRepository) : ICategoryService
    {
        public async Task ProcessCategory(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("No category options specified!");
                return;
            }
            args = args.Skip(1).ToArray();
            var parameters = args.Skip(1).ToArray();

            switch (args![0])
            {
                case "-d":
                case "--display":
                    await GetCategories(parameters);
                    break;
                case "-c":
                case "--create":
                    await CreateCategoryStructure(parameters);
                    break;
                default:
                    Console.WriteLine("Invalid category option!");
                    break;
            }
        }

        private async Task GetCategories(string[] parameters)
        {
            string additionalParameters = string.Empty;

            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].Contains("="))
                    {
                        throw new ArgumentException($"Parameters must be in the format 'key=value' ({parameters[i]})");
                    }
                    var key = parameters[i].Split('=')[0];
                    var value = parameters[i].Split('=')[1];
                    additionalParameters += $"&criteria[{i}][key]={key}&criteria[{i}][value]={value}";
                }
            }

            string url = $"&wsfunction=core_course_get_categories{additionalParameters}";
            await httpService.Get(url);
        }

        private async Task CreateCategoryStructure(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("No category data specified!");
                return;
            }
            if (parameters.Length > 1)
            {
                Console.WriteLine("A single parameter in the format 'parameter=value' is required.");
                Console.WriteLine("(The 'parameter' can be 'id')");
                return;
            }
            if (!parameters[0].Contains("="))
            {
                Console.WriteLine("Parameters must be in the format 'parameter=value')");
                return;
            }

            var parameter = parameters[0].Split('=')[0];
            var value = parameters[0].Split('=')[1];

            switch (parameter)
            {
                case "id":
                    int elfhComponentId = 0;
                    Int32.TryParse(value, out elfhComponentId);

                    if (elfhComponentId == 0)
                    {
                        Console.WriteLine("Invalid elfh component ID!");
                        return;
                    }
                    ElfhComponent elfhComponent = await componentRepository.GetByIdAsync(elfhComponentId);
                    elfhComponent.MoodleCategoryId = await CreateMoodleCategory(elfhComponent);

                    if (elfhComponent.MoodleCategoryId == 0)
                    {
                        Console.WriteLine("Category creation failed!");
                        return;
                    }

                    Console.WriteLine($"Would you like to create the child elfh components and add them to the '{elfhComponent.ComponentName}' category?");
                    Console.WriteLine("Press 'Y' to continue or any other key to exit.");

                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    Console.WriteLine();
                    if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
                    {
                        List<ElfhComponent> elfhChildComponents = await componentRepository.GetChildComponentsAsync(elfhComponent.ComponentId);
                        await CreateCategoryChildren(elfhComponent, elfhChildComponents);
                    }

                    break;
                default:
                    Console.WriteLine("Parameter must 'id'");
                    break;
            }
        }

        private async Task<int> CreateMoodleCategory(ElfhComponent? elfhComponent)
        {
            if (elfhComponent == null)
            {
                Console.WriteLine("Elfh component not found!");
                return 0;
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                    { "categories[0][name]", elfhComponent.ComponentName },
                    { "categories[0][parent]", elfhComponent.MoodleParentCategoryId.ToString() },
                    { "categories[0][idnumber]", $"elfh-{elfhComponent.ComponentId}" },
                    { "categories[0][description]", elfhComponent.ComponentDescription },
                    { "categories[0][descriptionformat]", "1" },
                    { "categories[0][theme]", "" }
                };

                Console.WriteLine($"Creating category '{elfhComponent.ComponentName}'");
                string url = "&wsfunction=core_course_create_categories";

                return await httpService.Post(url, parameters);
            }
        }

        private async Task CreateCategoryChildren(ElfhComponent elfhComponent, List<ElfhComponent> elfhChildComponents)
        {
            List<ElfhComponent> children = elfhChildComponents.Where(c => c.ParentComponentId == elfhComponent.ComponentId).ToList();
            if (children.Count > 0)
            {
                Console.WriteLine($"Processing {children.Count} child objects for '{elfhComponent.ComponentName}'");
            }
            foreach (var elfhChildComponent in children)
            {
                elfhChildComponent.MoodleParentCategoryId = elfhComponent.MoodleCategoryId;
                switch ((ComponentTypeEnum)elfhChildComponent.ComponentTypeId)
                {
                    case ComponentTypeEnum.ClinicalGroup:
                        Console.WriteLine($"Clinical group '{elfhChildComponent.ComponentName}'");
                        break;
                    case ComponentTypeEnum.Programme:
                    case ComponentTypeEnum.Folder:
                        Console.WriteLine($"Creating {children.Count} child categories for '{elfhComponent.ComponentName}'");
                        elfhChildComponent.MoodleCategoryId = await CreateMoodleCategory(elfhChildComponent);
                        await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                        break;
                    case ComponentTypeEnum.Application:
                        Console.WriteLine($"Application '{elfhChildComponent.ComponentName}'");
                        break;
                    case ComponentTypeEnum.Course:
                        Console.WriteLine($"Creating Course '{elfhChildComponent.ComponentName}'");
                        await CreateCourse(elfhChildComponent);
                        break;
                    case ComponentTypeEnum.LearningPath:
                        Console.WriteLine($"Creating Course for Learning Path '{elfhChildComponent.ComponentName}'");
                        await CreateCourse(elfhChildComponent);
                        break;
                    case ComponentTypeEnum.Session:
                        Console.WriteLine($"Session '{elfhChildComponent.ComponentName}'");
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task<int> CreateCourse(ElfhComponent elfhComponent)
        {
            if (elfhComponent == null)
            {
                Console.WriteLine("Elfh component not found!");
                return 0;
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "courses[0][fullname]", elfhComponent.ComponentName },
                    { "courses[0][shortname]", elfhComponent.ComponentName },
                    { "courses[0][categoryid]", elfhComponent.MoodleParentCategoryId.ToString() },
                    { "courses[0][idnumber]", $"elfh-{elfhComponent.ComponentId}" },
                    { "courses[0][summary]", elfhComponent.ComponentDescription },
                    { "courses[0][lang]", "en" }
                };

                //// Potential Course attributes
                //courses[0][fullname] = string
                //courses[0][shortname] = string
                //courses[0][categoryid] = int
                //courses[0][idnumber] = string
                //courses[0][summary] = string
                //courses[0][summaryformat] = int
                //courses[0][format] = string
                //courses[0][showgrades] = int
                //courses[0][newsitems] = int
                //courses[0][startdate] = int
                //courses[0][enddate] = int
                //courses[0][numsections] = int
                //courses[0][maxbytes] = int
                //courses[0][showreports] = int
                //courses[0][visible] = int
                //courses[0][hiddensections] = int
                //courses[0][groupmode] = int
                //courses[0][groupmodeforce] = int
                //courses[0][defaultgroupingid] = int
                //courses[0][enablecompletion] = int
                //courses[0][completionnotify] = int
                //courses[0][lang] = string
                //courses[0][forcetheme] = string
                //courses[0][courseformatoptions][0][name] = string
                //courses[0][courseformatoptions][0][value] = string
                //courses[0][customfields][0][shortname] = string
                //courses[0][customfields][0][value] = string

                string url = "&wsfunction=core_course_create_courses";

                return await httpService.Post(url, parameters);
            }
        }
    }
}