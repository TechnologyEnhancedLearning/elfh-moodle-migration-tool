using Microsoft.EntityFrameworkCore;
using Moodle_Migration_WebUI.Models;

namespace Moodle_Migration_WebUI.Interfaces
{
    public interface ILoggingRepository
    {
        Task<LoggingModel> InsertLog(LoggingModel log);
        
    }
}
