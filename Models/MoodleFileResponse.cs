using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodle_Migration.Models
{
    public class MoodleFileResponse
    {
        public string itemid { get; set; }
        public string filename { get; set; }
        public string filepath { get; set; }
        public string fileurl { get; set; }
        public string mimetype { get; set; }
        public int userid { get; set; }
        public string author { get; set; }
        public string license { get; set; }
        public long timecreated { get; set; }
        public long timemodified { get; set; }
    }
}
