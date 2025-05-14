namespace Moodle_Migration.Models
{
    public class ElfhComponent
    {
        public int ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string ComponentDescription { get; set; } = string.Empty;
        public int ParentComponentId { get; set; }
        public int ComponentTypeId { get; set; }
        public int Position { get; set; }
        public int RootComponentId { get; set; }
        public int MoodleCategoryId { get; set; }
        public int MoodleParentCategoryId { get; set; }
        public int MoodleCourseId { get; set; }
        public string DevelopmentId { get; set; } = string.Empty;
    }
}
