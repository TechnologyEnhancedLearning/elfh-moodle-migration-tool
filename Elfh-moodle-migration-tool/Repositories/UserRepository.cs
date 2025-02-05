using Dapper;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Data;

namespace Moodle_Migration.Repositories
{
    public class UserRepository(IDbConnection dbConnection) : IUserRepository
    {

        public async Task<ElfhUser> GetByIdAsync(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            var user = await dbConnection.QueryFirstOrDefaultAsync<ElfhUser>("dbo.proc_UserLoadByUserId", parameters, commandType: CommandType.StoredProcedure);
            return user!;
        }

        public async Task<List<ElfhUser>> SearchAsync(ElfhUserSearchModel searchModel)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", searchModel.UserName);
	        parameters.Add("@FirstName", searchModel.FirstName);
            parameters.Add("@LastName", searchModel.LastName);
            parameters.Add("@EmailAddress", searchModel.EmailAddress);
            parameters.Add("@PreferredName", searchModel.PreferredName);
            parameters.Add("@CountryId", searchModel.CountryId == 0 ? null : searchModel.CountryId);
            parameters.Add("@DefaultProjectId", searchModel.DefaultProjectId);
            parameters.Add("@searchUserGroupId", searchModel.SearchUserGroupId);
            parameters.Add("@searchUserTypeUserGroupId", searchModel.SearchUserTypeUserGroupId);
            parameters.Add("@Page", searchModel.Page == 0 ? 1 : searchModel.Page);
            parameters.Add("@PageSize", searchModel.PageSize == 0 ? 99999 : searchModel.PageSize);
            parameters.Add("@IncludeDeletedAccs", searchModel.IncludeDeletedAccs);
            parameters.Add("@PagesReturned", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var elfhUsers = await dbConnection.QueryAsync<ElfhUser>("dbo.proc_UserSearch", parameters, commandType: CommandType.StoredProcedure);
            int pagesReturned = parameters.Get<int>("@PagesReturned");
            return elfhUsers.Cast<ElfhUser>().ToList();
        }

        public async Task<ElfhUser> GetByUserNameAsync(string userName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userName", userName);

            var user = await dbConnection.QueryFirstOrDefaultAsync<ElfhUser>("dbo.proc_UserLoadByUserName", parameters, commandType: CommandType.StoredProcedure);
            return user!;
        }
    }
}
