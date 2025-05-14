using Moodle_Migration.Interfaces;

namespace Moodle_Migration.Services
{
    public class CourseService(IHttpService httpService) : ICourseService
    {
        public async Task<string> ProcessCourse(string[] args)
        {
            string result = string.Empty;
            if (args.Length < 2)
            {
                result = "No course options specified!";                
            }

            if (string.IsNullOrEmpty(result))
            {
                string[] parameters = args.Skip(2).ToArray();

                switch (args![1])
                {
                    case "-d":
                    case "--display":
                        result = await GetCourses(parameters);
                        break;
                    case "-c":
                    case "--create":
                        result = "Course creation is part of category child creation (-ct -c).";
                        break;
                    default:
                        result = "Invalid course option!";
                        break;
                }
            }

            Console.Write(result);
            Console.WriteLine();
            return result;
        }

        private async Task<string> GetCourses(string[] parameters)
        {
            string additionalParameters = string.Empty;

            if (parameters.Length == 1) // field and value are provided
            {
                if (!parameters[0].Contains("="))
                {
                    return ($"Parameters must be in the format 'field=value' ({parameters[0]})");
                }
                var key = parameters[0].Split('=')[0];
                var value = parameters[0].Split('=')[1];
                additionalParameters = $"&field={key}&value={value}";
            }

            string url = $"&wsfunction=core_course_get_courses_by_field{additionalParameters}";
            return await httpService.Get(url);
        }
    }
}