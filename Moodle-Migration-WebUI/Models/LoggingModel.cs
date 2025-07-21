using System.ComponentModel.DataAnnotations;

namespace Moodle_Migration_WebUI.Models
{
    public class LoggingModel
    {
        [Key]
        public int Id { get; set; }  // Primary key
        public DateTime MigrationDateTime { get; set; }
        public int SourceComponentId { get; set; }
        public int SourceComponentHierarchyId { get; set; }
        public int SourceParentComponentId { get; set; }
        public int SourceProgrammeComponentId { get; set; }
        public int SourceCourseComponentId { get; set; }
        public string SourceDevelopmentId { get; set; } = string.Empty;
        public DateTimeOffset SourceAmendDate { get; set; }
        public int DestinationCourseCategoriesId { get; set; }
        public int DesitinationCourseSectionsId { get; set; }
        public int DestinationCourseId { get; set; }
        public int DestinationScormId { get; set; }
        public int CreateUser { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset AmendDate { get; set; }
    }
}
