using Moodle_Migration.Models;

namespace Moodle_Migration.Interfaces
{
    public interface ICohortService
    {
        Task<List<MoodleCohort>> GetCohortsAsync(int instanceId);
        Task<MoodleCohort?> GetCohortByIdAsync(int cohortId, int instanceId);
        Task<MoodleCohort?> GetCohortByIdNumberAsync(string idNumber, int instanceId);
        Task<List<MoodleCohort>> SearchCohortsAsync(string searchTerm, int instanceId);
    }
}
