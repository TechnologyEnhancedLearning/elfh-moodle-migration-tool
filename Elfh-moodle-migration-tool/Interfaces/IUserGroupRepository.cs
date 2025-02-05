using Moodle_Migration.Models;

namespace Moodle_Migration.Interfaces
{
    public interface IUserGroupRepository
    {
        Task<ElfhUserGroup> GetByIdAsync(int userId);
    }
}
