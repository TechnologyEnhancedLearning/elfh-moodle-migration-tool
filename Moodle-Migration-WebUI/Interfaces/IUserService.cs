namespace Moodle_Migration.Interfaces
{
    public interface IUserService
    {
        Task<string> ProcessUser(string[] args);
    }
}
