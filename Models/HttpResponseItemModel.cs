using System.Text.Json.Serialization;

namespace Moodle_Migration.Models
{
    public class  HttpResponseItemModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string UserName { get; set; } = string.Empty;    
    }
}
