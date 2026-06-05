namespace Moodle_Migration.Models
{
    public class MoodleCohort
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int MemberCount { get; set; }
    }
}
