namespace Moodle_Migration.Interfaces
{
    public interface ICategoryService
    {
        Task<string> ProcessCategory(string[] args);
    }
}
