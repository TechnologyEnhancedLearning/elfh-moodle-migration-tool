using Moodle_Migration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodle_Migration.Interfaces
{
    public interface IContentObjectRepository
    {
        Task<ElfhComponent> GetByContentObjectIdsync(int contentObjectId);
    }
}
