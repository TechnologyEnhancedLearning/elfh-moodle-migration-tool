namespace Moodle_Migration_WebUI.Interfaces
{
    public interface ICommandService
    {
        Task<string> ExecuteCommand(string input);
    }
}
