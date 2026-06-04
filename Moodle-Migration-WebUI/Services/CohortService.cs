using Microsoft.Extensions.Configuration;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System.Text.Json;

namespace Moodle_Migration.Services
{
    public class CohortService : ICohortService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private string defaultParameters;

        public CohortService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            // Create an HttpClient instance
            _client = _httpClientFactory.CreateClient("MoodleClient");

            defaultParameters = $"?wstoken={_configuration["MoodleApi:wstoken"]}"
                              + $"&moodlewsrestformat={_configuration["MoodleApi:moodlewsrestformat"]}";
        }

        public async Task<List<MoodleCohort>> GetCohortsAsync()
        {
            var cohorts = new List<MoodleCohort>();
            try
            {
                string url = "&wsfunction=core_cohort_get_cohorts";
                HttpResponseMessage response = await _client.GetAsync(defaultParameters + url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in doc.RootElement.EnumerateArray())
                        {
                            var cohort = new MoodleCohort
                            {
                                Id = element.GetProperty("id").GetInt32(),
                                Name = element.GetProperty("name").GetString() ?? string.Empty,
                                IdNumber = element.GetProperty("idnumber").GetString() ?? string.Empty,
                                Description = element.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
                                CategoryId = element.TryGetProperty("categoryid", out var catId) ? catId.GetInt32() : 0,
                                MemberCount = 0
                            };
                            cohorts.Add(cohort);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error retrieving cohorts: {e.Message}");
            }
            return cohorts;
        }

        public async Task<MoodleCohort?> GetCohortByIdAsync(int cohortId)
        {
            try
            {
                var cohorts = await GetCohortsAsync();
                return cohorts.FirstOrDefault(c => c.Id == cohortId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving cohort by ID: {e.Message}");
                return null;
            }
        }

        public async Task<MoodleCohort?> GetCohortByIdNumberAsync(string idNumber)
        {
            try
            {
                var cohorts = await GetCohortsAsync();
                return cohorts.FirstOrDefault(c => c.IdNumber == idNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving cohort by ID number: {e.Message}");
                return null;
            }
        }

        public async Task<List<MoodleCohort>> SearchCohortsAsync(string searchTerm)
        {
            try
            {
                var allCohorts = await GetCohortsAsync();
                var filtered = allCohorts.Where(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.IdNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                return filtered;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error searching cohorts: {e.Message}");
                return new List<MoodleCohort>();
            }
        }
    }
}
