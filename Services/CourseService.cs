using Moodle_Migration.Interfaces;

namespace Moodle_Migration.Services
{
    public class CourseService(IHttpService httpService) : ICourseService
    {
        public async Task ProcessCourse(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("No course options specified!");
                return;
            }

            string[] parameters = args.Skip(2).ToArray();

            switch (args![1])
            {
                case "-d":
                case "--display":
                    await GetCourses(parameters);
                    break;
                case "-c":
                case "--create":
                    Console.WriteLine("Course creation is part of category child creation (-ct -c).");
                    break;
                default:
                    Console.WriteLine("Invalid course option!");
                    break;
            }
        }

        private async Task GetCourses(string[] parameters)
        {
            string additionalParameters = string.Empty;

            if (parameters.Length == 1) // field and value are provided
            {
                if (!parameters[0].Contains("="))
                {
                    throw new ArgumentException($"Parameters must be in the format 'field=value' ({parameters[0]})");
                }
                var key = parameters[0].Split('=')[0];
                var value = parameters[0].Split('=')[1];
                additionalParameters = $"&field={key}&value={value}";
            }

            string url = $"&wsfunction=core_course_get_courses_by_field{additionalParameters}";
            await httpService.Get(url);
        }
    }
}