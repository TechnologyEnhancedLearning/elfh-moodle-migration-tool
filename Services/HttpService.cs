using Microsoft.Extensions.Configuration;
using Moodle_Migration.Interfaces;
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

        public async Task Post(string url)
        {
            try
            {
                //// Create the content for the POST request
                //var content = new FormUrlEncodedContent(parameters);

                // Make the POST request
                HttpResponseMessage response = await _client.PostAsync(url, null);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();

                // Format the response using System.Text.Json and output
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
    }
}
