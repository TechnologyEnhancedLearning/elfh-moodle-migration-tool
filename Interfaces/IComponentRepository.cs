using Moodle_Migration.Models;

namespace Moodle_Migration.Interfaces
{
    public interface IComponentRepository
    {
        Task<ElfhComponent> GetByIdAsync(int componentId);
        Task<List<ElfhComponent>> GetChildComponentsAsync(int componentId);
    }
}
