using Moodle_Migration.Models;

namespace Moodle_Migration.Interfaces
{
    public interface IUserRepository
    {
        Task<ElfhUser> GetByIdAsync(int userId);
        Task<ElfhUser> GetByUserNameAsync(string userName);
        Task<List<ElfhUser>> SearchAsync(ElfhUserSearchModel searchModel);
    }
}
