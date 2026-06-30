namespace Moodle_Migration.Interfaces
{
    public interface IHttpService
    {
        Task<string> Get(string url, int instanceId);
        Task<(string result,int resultValue)> Post(string url, Dictionary<string, string> parameters,int instanceId);
        Task<(string result, int scormId, int sectionId)> PostScorm(string url, Dictionary<string, string> parameters, int instanceId);
    }
}
