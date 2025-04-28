using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodle_Migration.Interfaces
{
    public interface IFileService
    {
        Task<byte[]> DownloadFileAsync(string developmentId);
    }
}
