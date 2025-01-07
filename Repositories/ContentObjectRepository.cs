using Dapper;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodle_Migration.Repositories
{
    public class ContentObjectRepository(IDbConnection dbConnection) : IContentObjectRepository
    {
        public async Task<ElfhComponent> GetByContentObjectIdsync(int contentObjectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@contentObjectId", contentObjectId);

            var component = await dbConnection.QueryFirstOrDefaultAsync<ElfhComponent>("dbo.proc_ContentObjectLoadByContentObjectId", parameters, commandType: CommandType.StoredProcedure);
            return component!;
        }
    }
}
