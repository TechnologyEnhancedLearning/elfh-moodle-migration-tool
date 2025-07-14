using Moodle_Migration.Models;
using Moodle_Migration_WebUI.Models;

namespace Moodle_Migration.Interfaces
{
    public interface IComponentRepository
    {
        Task<ElfhComponent> GetByIdAsync(int componentId);
        Task<List<ElfhComponent>> GetChildComponentsAsync(int componentId);
        Task<LoggingModel> GetScormDataAsync(int elfhComponentId);
    }
}
