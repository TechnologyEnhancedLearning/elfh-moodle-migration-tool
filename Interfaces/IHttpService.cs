namespace Moodle_Migration.Interfaces
{
    public interface IHttpService
    {
        Task Get(string url);
        Task<int> Post(string url, Dictionary<string, string> parameters);
    }
}
