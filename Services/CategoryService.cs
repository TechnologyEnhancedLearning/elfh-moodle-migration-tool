using Moodle_Migration.Interfaces;

namespace Moodle_Migration.Services
{
    public class CategoryService(IHttpService httpService) : ICategoryService
    {
        public async Task ProcessCategory(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("No category options specified!");
                return;
            }

            string[] parameters = args.Skip(2).ToArray();

            switch (args![1])
            {
                case "-d":
                case "--display":
                    await GetCategories(parameters);
                    break;
                case "-c":
                case "--create":
                    Console.WriteLine("Create category - NOT YET IMPLEMENTED");
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
    }
}