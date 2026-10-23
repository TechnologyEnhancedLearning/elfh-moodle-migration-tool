using Microsoft.EntityFrameworkCore;
using Moodle_Migration_WebUI.Models;

namespace Moodle_Migration_WebUI.Interfaces
{
    public interface ILoggingRepository
    {
        Task<LoggingModel> InsertLog(LoggingModel log);
        Task<DateTime?> GetLastUserGroupSyncDateTime(int userGroupId, int instanceId);
        Task InsertUserGroupSyncLog(int userGroupId, DateTime syncDateTime, int instanceId);
    }
}
