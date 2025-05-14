namespace Moodle_Migration.Models
{
    public class ElfhUserSearchModel
    {
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PreferredName { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public int DefaultProjectId { get; set; }
        public int SearchUserGroupId { get; set; }
        public int SearchUserTypeUserGroupId { get; set; }
        public int Page { get; set; }
        public float PageSize { get; set; }
        public bool IncludeDeletedAccs { get; set; }
    }
}
