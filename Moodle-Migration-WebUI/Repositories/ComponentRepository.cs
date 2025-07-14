using Dapper;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using Moodle_Migration_WebUI.Models;
using System.Data;

namespace Moodle_Migration.Repositories
{
    public class ComponentRepository(IDbConnection dbConnection) : IComponentRepository
    {

        public async Task<ElfhComponent> GetByIdAsync(int componentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@componentId", componentId);

            var component = await dbConnection.QueryFirstOrDefaultAsync<ElfhComponent>("dbo.proc_ComponentLoadByComponentId", parameters, commandType: CommandType.StoredProcedure);
            return component!;
        }

        public async Task<List<ElfhComponent>> GetChildComponentsAsync(int componentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProgrammeId", componentId);
            parameters.Add("@UserId", 4); // Portal Admin userid

            var elfhComponents = await dbConnection.QueryAsync<ElfhComponent>("dbo.proc_MyELearningProgrammeChildComponentsAll", parameters, commandType: CommandType.StoredProcedure);
            return elfhComponents.Cast<ElfhComponent>().ToList();
        }
        
    }
}
