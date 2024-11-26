namespace Moodle_Migration.Interfaces
{
    public interface ICourseService
    {
        Task ProcessCourse(string[] args);
    }
}
