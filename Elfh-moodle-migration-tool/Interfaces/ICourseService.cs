namespace Moodle_Migration.Interfaces
{
    public interface ICourseService
    {
        Task<string> ProcessCourse(string[] args);
    }
}
