using System.Text.Json.Serialization;

namespace Moodle_Migration_WebUI.Models
{
    public class HttpResponseItemScormModel
    {
        [JsonPropertyName("scormid")]
        public int ScormId { get; set; }

        [JsonPropertyName("sectionid")]
        public int Sectionid { get; set; }
    }
}
