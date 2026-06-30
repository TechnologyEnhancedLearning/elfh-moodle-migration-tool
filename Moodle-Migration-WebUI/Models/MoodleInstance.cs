namespace Moodle_Migration_WebUI.Models
{
    public class MoodleInstance
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string BaseUrl { get; set; } = "";

        public string WsToken { get; set; } = "";

        public string RestFormat { get; set; } = "json";

        public bool IsActive { get; set; } = true;
    }
}
