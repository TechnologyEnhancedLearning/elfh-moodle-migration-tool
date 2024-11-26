namespace Moodle_Migration.Interfaces
{
    public interface ICategoryService
    {
        Task ProcessCategory(string[] args);
    }
}
