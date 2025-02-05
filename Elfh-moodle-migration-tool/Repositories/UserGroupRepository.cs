using Dapper;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Data;

namespace Moodle_Migration.Repositories
{
    public class UserGroupRepository(IDbConnection dbConnection) : IUserGroupRepository
    {

        public async Task<ElfhUserGroup> GetByIdAsync(int userGroupId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userGroupId", userGroupId);

            var user = await dbConnection.QueryFirstOrDefaultAsync<ElfhUserGroup>("dbo.proc_UserGroupLoadByUserGroupId", parameters, commandType: CommandType.StoredProcedure);
            return user!;
        }
    }
}
