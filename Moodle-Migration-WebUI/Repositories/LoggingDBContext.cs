using System;
using Microsoft.EntityFrameworkCore;
using Moodle_Migration_WebUI.Models;

namespace Moodle_Migration_WebUI.Repositories
{
    public partial class LoggingDBContext:DbContext
    {
        public LoggingDBContext(DbContextOptions<LoggingDBContext> options)
        : base(options) { }

        public DbSet<LoggingModel> MigrationLog { get; set; }
        public DbSet<User> UserTBL { get; set; }
    }
}
