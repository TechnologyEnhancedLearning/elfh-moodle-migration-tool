namespace Moodle_Migration.Interfaces
{
    public interface IHttpService
    {
        Task Get(string url);
        Task Post(string url);
    }
}
