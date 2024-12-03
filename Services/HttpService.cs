using Microsoft.Extensions.Configuration;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Text.Json;

namespace Moodle_Migration.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private string defaultParameters;

        public HttpService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            // Create an HttpClient instance
            _client = _httpClientFactory.CreateClient("MoodleClient");

            defaultParameters = $"?wstoken={_configuration["MoodleApi:wstoken"]}"
                              + $"&moodlewsrestformat={_configuration["MoodleApi:moodlewsrestformat"]}";
        }

        public async Task Get(string url)
        {
            try
            {
                // Make the GET request
                HttpResponseMessage response = await _client.GetAsync(defaultParameters + url);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();


                // Format the response using System.Text.Json amd output
                string formattedResponseBody = JsonSerializer.Serialize(JsonDocument.Parse(responseBody), new JsonSerializerOptions { WriteIndented = true });
                Console.Write(formattedResponseBody);
                Console.WriteLine();
            }
            catch (HttpRequestException e)
            {
                // Handle any errors that occurred during the request
                Console.WriteLine($"Request error: {e.Message}");
            }
        }

        public async Task<int> Post(string url, Dictionary<string, string> parameters)
        {
            try
            {
                int returnId = 0;
                // Create the content for the POST request
                var content = new MultipartFormDataContent();
                foreach (var parameter in parameters)
                {
                    content.Add(new StringContent(parameter.Value), parameter.Key);
                }

                // Make the POST request with the content
                HttpResponseMessage response = await _client.PostAsync(defaultParameters + url, content);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Deserialize the response content into HttpResponseItemModel list
                        List<HttpResponseItemModel>? items = JsonSerializer.Deserialize<List<HttpResponseItemModel>>(responseBody);
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Console.WriteLine($"Id: {item.Id} created in Moodle");
                            }
                            if (items.Count == 1)
                            {
                                returnId = items[0].Id;
                            }
                        }
                    }
                    else
                    {
                        string formattedResponseBody = JsonSerializer.Serialize(JsonDocument.Parse(responseBody), new JsonSerializerOptions { WriteIndented = true });
                        Console.Write(formattedResponseBody);
                    }
                }
                Console.WriteLine();

                return returnId;
            }
            catch (HttpRequestException e)
            {
                // Handle any errors that occurred during the request
                Console.WriteLine($"Request error: {e.Message}");
                return 0;
            }
        }
    }
}
