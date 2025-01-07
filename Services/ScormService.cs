using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Moodle_Migration.Services
{
    public class ScormService(IHttpService httpService, IUserRepository userRepository, IUserGroupRepository userGroupRepository) : IScormService
    {
        public async Task ProcessScorm(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("No category options specified!");
                return;
            }
            args = args.Skip(1).ToArray();
            var parameters = args.Skip(1).ToArray();

            switch (args![0])
            {
                case "-d":
                case "--display":
                    // await GetCategories(parameters);
                    break;
                case "-c":
                case "--create":
                    // await CreateCategoryStructure(parameters);
                    string scormFilePath = "C:\\Users\binon.yesudhas\\Downloads\\AllGolfExamples\\xxx_91_195.zip";
                    UploadScormToMoodle("http://localhost", "524a07e0cdf41f22c20eb974a4614d9b", "10003", scormFilePath);
                    break;
                default:
                    Console.WriteLine("Invalid scorm option!");
                    break;
            }
        }


        async Task UploadScormToMoodle(string moodleUrl, string token, string courseId, string scormFilePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Step 1: Upload File
                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(token), "token");
                    //  formData.Add(new StreamContent(File.OpenRead(scormFilePath)), "file", Path.GetFileName(scormFilePath));

                    //HttpResponseMessage uploadResponse = await client.PostAsync($"{moodleUrl}/webservice/upload.php", formData);
                    //string uploadResult = await uploadResponse.Content.ReadAsStringAsync();

                    //// Parse the file upload response to get the file URL (you may need to handle JSON here)
                    //var files = JsonConvert.DeserializeObject<List<MoodleFileResponse>>(uploadResult);
                    // var fileUrl = files.Find(f => f.Component == "mod_scorm" && f.Filearea == "content").ItemUri;

                    // Step 2: Add SCORM to Course
                    int sectionId = 1; // Section ID where SCORM will be added
                    string scormUrl = "https://yourserver.com/path/to/scorm.zip";
                    string scormName = "Binon SCORM Activity";

                    var activities = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>
            {
                { "module", "scorm" },  // SCORM module
                { "name", scormName },  // SCORM module name
                { "intro", "scormDescription" },  // SCORM module description
                { "packageurl", scormUrl } // URL to SCORM package
            }
        };

                    // Prepare the form data for updating the course
                    var scormData = new FormUrlEncodedContent(new[]
                    {
            new KeyValuePair<string, string>("wstoken", token),
            new KeyValuePair<string, string>("wsfunction", "core_course_update_courses"),
            new KeyValuePair<string, string>("moodlewsrestformat", "json"),
            new KeyValuePair<string, string>("courses[0][id]", courseId.ToString()), // The existing course ID
            new KeyValuePair<string, string>("courses[0][modules]", activities.ToString()) // Adding SCORM module to course
        });

                    //            var scormData = new FormUrlEncodedContent(new[]
                    //{
                    //    new KeyValuePair<string, string>("wstoken", token),
                    //    new KeyValuePair<string, string>("wsfunction", "core_course_create_module"),
                    //    new KeyValuePair<string, string>("moodlewsrestformat", "json"),

                    //    // Module details
                    //    new KeyValuePair<string, string>("courseid", courseId.ToString()),
                    //    new KeyValuePair<string, string>("section", sectionId.ToString()),
                    //    new KeyValuePair<string, string>("modulename", "scorm"), // Module type
                    //    new KeyValuePair<string, string>("instance[name]", scormName), // SCORM name
                    //    new KeyValuePair<string, string>("instance[intro]", "scormDescription"), // SCORM description
                    //    new KeyValuePair<string, string>("instance[packageurl]", scormUrl) // SCORM package URL
                    //});



                    //             var scormData = new FormUrlEncodedContent(new[]
                    //{
                    //     new KeyValuePair<string, string>("wstoken", token),
                    //     new KeyValuePair<string, string>("wsfunction", "core_course_create_module"),
                    //     new KeyValuePair<string, string>("courseid", courseId.ToString()), // Target course ID
                    //     new KeyValuePair<string, string>("modulename", "scorm"), // Module type is SCORM
                    //     new KeyValuePair<string, string>("name", "BIon SCORM Package"), // SCORM activity name
                    //     new KeyValuePair<string, string>("section", "1"), // Section ID in the course
                    //     new KeyValuePair<string, string>("intro", "This is a SCORM activity."), // Activity description
                    //     new KeyValuePair<string, string>("showdescription", "1"), // Show description
                    //     new KeyValuePair<string, string>("packageurl", "index_lms.html") // URL of the SCORM package
                    // });


                    //            var scormData = new FormUrlEncodedContent(new[]
                    //            {
                    //    new KeyValuePair<string, string>("wstoken", token),
                    //    new KeyValuePair<string, string>("wsfunction", "mod_scorm_add_scorm"),
                    //    new KeyValuePair<string, string>("courseid", courseId),
                    //    new KeyValuePair<string, string>("scormname", "My SCORM Package"),
                    //    new KeyValuePair<string, string>("fileurl", "index_lms.html") // From upload step
                    //});

                    HttpResponseMessage addScormResponse = await client.PostAsync($"{moodleUrl}/webservice/rest/server.php", scormData);
                    string addScormResult = await addScormResponse.Content.ReadAsStringAsync();

                    // await httpService.Post($"{moodleUrl}/webservice/rest/server.php", scormData);

                    // Handle response
                    Console.WriteLine(addScormResult);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
