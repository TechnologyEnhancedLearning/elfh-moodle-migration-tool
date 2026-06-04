using Moodle_Migration.Models;

namespace Moodle_Migration.Interfaces
{
    public interface ICohortService
    {
        Task<List<MoodleCohort>> GetCohortsAsync();
        Task<MoodleCohort?> GetCohortByIdAsync(int cohortId);
        Task<MoodleCohort?> GetCohortByIdNumberAsync(string idNumber);
        Task<List<MoodleCohort>> SearchCohortsAsync(string searchTerm);
    }
}
