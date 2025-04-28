using Microsoft.SqlServer.Server;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Security.Cryptography.X509Certificates;

namespace Moodle_Migration.Services
{
    public class CategoryService(IHttpService httpService, IComponentRepository componentRepository, IFileService fileService) : ICategoryService
    {
        public async Task<string> ProcessCategory(string[] args)
        {
            string result = string.Empty;
            if (args.Length < 2)
            {
                result = "No category options specified!";
            }
            if (string.IsNullOrEmpty(result))
            {
                args = args.Skip(1).ToArray();
                var parameters = args.Skip(1).ToArray();

                switch (args![0])
                {
                    case "-d":
                    case "--display":
                        result = await GetCategories(parameters);
                        break;
                    case "-c":
                    case "--create":
                        result = await CreateCategoryStructure(parameters);
                        break;
                    default:
                        result = "Invalid category option!";
                        break;
                }
            }

            Console.Write(result);
            Console.WriteLine();
            return result;
        }

        private async Task<string> GetCategories(string[] parameters)
        {
            string additionalParameters = string.Empty;

            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].Contains("="))
                    {
                        return ($"Parameters must be in the format 'key=value' ({parameters[i]})");
                    }
                    var key = parameters[i].Split('=')[0];
                    var value = parameters[i].Split('=')[1];
                    additionalParameters += $"&criteria[{i}][key]={key}&criteria[{i}][value]={value}";
                }
            }

            string url = $"&wsfunction=core_course_get_categories{additionalParameters}";
            return await httpService.Get(url);
        }

        private async Task<string> CreateCategoryStructure(string[] parameters)
        {
            string result = string.Empty;

            if (parameters.Length == 0)
            {
                return "No category data specified!";
            }
            if (parameters.Length > 1)
            {               
                return "A single parameter in the format 'parameter=value' is required. (The 'parameter' can be 'id') \n";
            }
            if (!parameters[0].Contains("="))
            {
                return "Parameters must be in the format 'parameter=value')";
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
                        return "Invalid elfh component ID!";
                    }
                    ElfhComponent elfhComponent = await componentRepository.GetByIdAsync(elfhComponentId);
                    if (elfhComponent == null)
                    {
                        return "Invalid elfh component ID!";
                    }
                    var categoryResult = await CreateMoodleCategory(elfhComponent);
                    elfhComponent.MoodleCategoryId = categoryResult.resultValue;
                    result = categoryResult.result;

                    if (elfhComponent.MoodleCategoryId == 0)
                    {
                        return "Category creation failed!";                        
                    }

                    Console.WriteLine($"Would you like to create the child elfh components and add them to the '{elfhComponent.ComponentName}' category?");
                    Console.WriteLine("Press 'Y' to continue or any other key to exit.");

                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    Console.WriteLine();
                    if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
                    {
                        List<ElfhComponent> elfhChildComponents = await componentRepository.GetChildComponentsAsync(elfhComponent.ComponentId);
                        result += await CreateCategoryChildren(elfhComponent, elfhChildComponents);
                    }

                    break;

                case "idweb":
                    elfhComponentId = 0;
                    Int32.TryParse(value, out elfhComponentId);

                    if (elfhComponentId == 0)
                    {
                        return "Invalid elfh component ID!";                        
                    }
                    elfhComponent = await componentRepository.GetByIdAsync(elfhComponentId);
                    if (elfhComponent == null)
                    {
                        return "Invalid elfh component ID!";
                    }
                    categoryResult = await CreateMoodleCategory(elfhComponent);
                    elfhComponent.MoodleCategoryId = categoryResult.resultValue;
                    result = categoryResult.result;

                    if (elfhComponent.MoodleCategoryId == 0)
                    {
                        result += "\n" + "Category creation failed!";
                        return result;
                    }

                    result += "\n" + $"The child elfh components will be created and added  to the '{elfhComponent.ComponentName}' category";

                    List<ElfhComponent> elfhChildComponentsWeb = await componentRepository.GetChildComponentsAsync(elfhComponent.ComponentId);
                    result += await CreateCategoryChildren(elfhComponent, elfhChildComponentsWeb);                    

                    break;
                default:
                    result = "Parameter must 'id'";
                    break;
            }
            return result;
        }

        private async Task<(string result, int resultValue)> CreateMoodleCategory(ElfhComponent? elfhComponent)
        {
            if (elfhComponent == null)
            {
                return ("Elfh component not found!", 0);
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

        private async Task<(string result, int resultValue)> CreateMoodleFolder(ElfhComponent? elfhComponent)
        {
            if (elfhComponent == null)
            {
                return ("Elfh component not found!", 0);
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                   
                    { "parentcategoryid", elfhComponent.MoodleParentCategoryId.ToString() },
                     { "name", elfhComponent.ComponentName },
                    { "idnumber", "" },
                    { "description", "" }
                };

                Console.WriteLine($"Creating category '{elfhComponent.ComponentName}'");
                string url = "&wsfunction=local_custom_service_create_subfolder";

                var result = await httpService.Post(url, parameters);
                Console.WriteLine("Folder " + result.result);
                return result;
            }
        }


        private async Task<string> CreateCategoryChildren(ElfhComponent elfhComponent, List<ElfhComponent> elfhChildComponents)
        {
            string result = string.Empty;

            List<ElfhComponent> children = elfhChildComponents.Where(c => c.ParentComponentId == elfhComponent.ComponentId).ToList();
            if (children.Count > 0)
            {
                Console.WriteLine($"Processing {children.Count} child objects for '{elfhComponent.ComponentName}'");
            }
            foreach (var elfhChildComponent in children)
            {
                elfhChildComponent.MoodleParentCategoryId = elfhComponent.MoodleCategoryId;
                elfhChildComponent.MoodleCourseId = elfhComponent.MoodleCourseId;
                switch ((ComponentTypeEnum)elfhChildComponent.ComponentTypeId)
                {
                    case ComponentTypeEnum.ClinicalGroup:
                        Console.WriteLine($"Clinical group '{elfhChildComponent.ComponentName}'");
                        break;
                    case ComponentTypeEnum.Programme:
                    case ComponentTypeEnum.Folder:
                        Console.WriteLine($"Creating {children.Count} child categories for '{elfhComponent.ComponentName}'");
                        var categoryResult = await CreateMoodleCategory(elfhChildComponent);
                        elfhChildComponent.MoodleCategoryId = categoryResult.resultValue;
                        result += categoryResult.result;
                        await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                        break;
                    //case ComponentTypeEnum.Folder:
                    //    Console.WriteLine($"Creating {children.Count} child categories for '{elfhComponent.ComponentName}'");
                    //    var folderResult = await CreateMoodleFolder(elfhChildComponent);
                    //    elfhChildComponent.MoodleCategoryId = folderResult.resultValue;
                    //    result += folderResult.result;
                    //    await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                    //    break;
                    case ComponentTypeEnum.Application:
                        Console.WriteLine($"Application '{elfhChildComponent.ComponentName}'");
                        break;
                    case ComponentTypeEnum.Course:
                        Console.WriteLine($"Creating Course '{elfhChildComponent.ComponentName}'");
                        var course = await CreateCourse(elfhChildComponent);
                        elfhChildComponent.MoodleCourseId = course.resultValue;
                        result += course.result;
                        await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                        break;
                    case ComponentTypeEnum.LearningPath:
                        Console.WriteLine($"Creating Course for Learning Path '{elfhChildComponent.ComponentName}'");
                        var learningpath = await CreateCourse(elfhChildComponent);
                        elfhChildComponent.MoodleCourseId = learningpath.resultValue;
                        result += learningpath.result;
                        await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                        break;
                    case ComponentTypeEnum.Session:
                        Console.WriteLine($"Session '{elfhChildComponent.ComponentName}'");
                        var developmentId = await componentRepository.GetDevelopmentIdForComponentAsync(elfhChildComponent.ComponentId);
                        elfhChildComponent.DevelopmentId = developmentId;
                        result += await CreateScorm(elfhChildComponent);
                        await CreateCategoryChildren(elfhChildComponent, elfhChildComponents);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private async Task<(string result, int resultValue)> CreateCourse(ElfhComponent? elfhComponent)
        {
            if (elfhComponent == null)
            {
                return ("Elfh component not found!", 0);
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
                var result = await httpService.Post(url, parameters);
                return result;
            }
        }
        private async Task<string> CreateScorm(ElfhComponent elfhComponent)
        {
            var zipBytes = await fileService.DownloadFileAsync(elfhComponent.DevelopmentId);
            // Convert to Base64
            string base64Zip = Convert.ToBase64String(zipBytes);

            if (elfhComponent == null)
            {
                return "Elfh component not found!";
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "courseid", elfhComponent.MoodleCourseId.ToString() },
                    { "section", "0" },
                    { "scormname", elfhComponent.ComponentName },
                    { "foldername", elfhComponent.DevelopmentId },
                    { "base64Zip", base64Zip }
                };



                string url = "&wsfunction=mod_scorm_insert_scorm_resource";
                Console.WriteLine("Creating scorm resource in moodle");
                var result = await httpService.Post(url, parameters);
                Console.WriteLine("Scorm "+ result.result);
                return result.result;
            }
        }
    }
}