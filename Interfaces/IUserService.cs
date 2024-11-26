namespace Moodle_Migration.Interfaces
{
    public interface IUserService
    {
        Task ProcessUser(string[] args);
    }
}
