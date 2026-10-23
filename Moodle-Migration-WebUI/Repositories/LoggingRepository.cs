using Dapper;
using Microsoft.EntityFrameworkCore;
using Moodle_Migration_WebUI.Interfaces;
using Moodle_Migration_WebUI.Models;
using System;
using System.Data;

namespace Moodle_Migration_WebUI.Repositories
{
    public class LoggingRepository:ILoggingRepository
    {
        private readonly LoggingDBContext _context;
        public LoggingRepository(LoggingDBContext context)
        {
            _context = context;
        }
        public async Task<LoggingModel> InsertLog(LoggingModel log)
        {
            _context.MigrationLog.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        private static string UserGroupSyncKey(int userGroupId) => $"UserGroup:{userGroupId}";

        public async Task<DateTime?> GetLastUserGroupSyncDateTime(int userGroupId, int instanceId)
        {
            var key = UserGroupSyncKey(userGroupId);
            return await _context.MigrationLog
                .Where(l => l.SourceDevelopmentId == key)
                .OrderByDescending(l => l.MigrationDateTime)
                .Select(l => (DateTime?)l.MigrationDateTime)
                .FirstOrDefaultAsync();
        }

        public async Task InsertUserGroupSyncLog(int userGroupId, DateTime syncDateTime, int instanceId)
        {
            _context.MigrationLog.Add(new LoggingModel
            {
                MigrationDateTime = syncDateTime,
                SourceDevelopmentId = UserGroupSyncKey(userGroupId),
                SourceAmendDate = syncDateTime,
                MoodleInstanceId = instanceId,
                CreateUser = 4,
                CreateDate = DateTimeOffset.Now,
                AmendDate = DateTimeOffset.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}
