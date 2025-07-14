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
    }
}
