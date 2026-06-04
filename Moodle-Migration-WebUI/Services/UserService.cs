using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using Moodle_Migration_WebUI.Hubs;
using System;
using System.Drawing;
using System.Reflection;
using System.Text.Json;

namespace Moodle_Migration.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpService httpService;
        private readonly ICohortService cohortService;
        private readonly IUserRepository userRepository;
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IHubContext<StatusHub> hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IHttpService _httpService, ICohortService _cohortService, IUserRepository _userRepository, IUserGroupRepository _userGroupRepository, IHubContext<StatusHub> _hubContext, IHttpContextAccessor httpContextAccessor)
        {
            httpService = _httpService;
            cohortService = _cohortService;
            userRepository = _userRepository;
            userGroupRepository = _userGroupRepository;
            hubContext = _hubContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> ProcessUser(string[] args)
        {
            string result = string.Empty;
            if (args.Length < 2)
            {
                result = "No user option specified!";
            }

            if (string.IsNullOrEmpty(result))
            {
                args = args.Skip(1).ToArray();
                var parameters = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case "-d":
                    case "--details":
                        result = await GetUsers(parameters);
                        break;
                    case "-c":
                    case "--create":
                        result = await CreateUsers(parameters);
                        break;
                    default:
                        result = "Invalid user option!";
                        break;
                }
            }

            Console.Write(result);
            Console.WriteLine();
            return result;
        }

        private async Task<string> GetUsers(string[] parameters)
        {
            string additionalParameters = string.Empty;

            if (parameters.Length == 0)
            {
                additionalParameters = "&criteria[0][key]=&criteria[0][value]=";
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].Contains("="))
                    {
                        return ($"Parameters must be in the format 'key=value' ({parameters[i]})");
                    }
                    var key = parameters[i].Split('=')[0];
                    var value = parameters[i].Split('=')[1];
                    additionalParameters += $"&criteria[{i}][key]={key}&criteria[{i}][value]={value}";
                }
            }

            string url = $"&wsfunction=core_user_get_users{additionalParameters}";
            return await httpService.Get(url);
        }

        private async Task<string> CreateUsers(string[] parameters)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            string result = string.Empty;

            if (parameters.Length == 0)
            {
                return "No user data specified!";
            }

            // Parse parameters - allow multiple key=value pairs
            var parsedParams = new Dictionary<string, string>();
            foreach (var param in parameters)
            {
                if (!param.Contains("="))
                {
                    return "Parameters must be in the format 'parameter=value'";
                }
                var parts = param.Split('=');
                if (parts.Length == 2)
                {
                    parsedParams[parts[0]] = parts[1];
                }
            }

            if (parsedParams.Count == 0)
            {
                return "No valid parameters found!";
            }

            ElfhUser? elfhUser = null;

            // Handle single user migration (id or username)
            if (parsedParams.ContainsKey("id"))
            {
                int elfhUserId = 0;
                Int32.TryParse(parsedParams["id"], out elfhUserId);

                if (elfhUserId == 0)
                {
                    return "Invalid user ID!";
                }
                elfhUser = await userRepository.GetByIdAsync(elfhUserId);
                result = await CreateElfhUser(elfhUser);
            }
            else if (parsedParams.ContainsKey("username"))
            {
                var value = parsedParams["username"];
                if (value.IsNullOrEmpty())
                {
                    return "Empty username!";
                }
                elfhUser = await userRepository.GetByUserNameAsync(value);
                result = await CreateElfhUser(elfhUser);
            }
            else if (parsedParams.ContainsKey("ugidweb") || parsedParams.ContainsKey("ugid"))
            {
                // Handle user group migration
                var ugidKey = parsedParams.ContainsKey("ugidweb") ? "ugidweb" : "ugid";
                int elfhUserGroupId = 0;
                Int32.TryParse(parsedParams[ugidKey], out elfhUserGroupId);

                if (elfhUserGroupId == 0)
                {
                    return "Invalid user group ID!";
                }

                var elfhUserGroup = await userGroupRepository.GetByIdAsync(elfhUserGroupId);

                if (elfhUserGroup == null)
                {
                    return "User group not found!";
                }

                // Determine target cohort
                string targetCohortIdentifier = null;
                bool createdNewCohort = false;

                if (parsedParams.ContainsKey("ugid"))
                {
                    targetCohortIdentifier = "elfh-" + parsedParams["ugid"];
                }
                else if (parsedParams.ContainsKey("ugidweb"))
                {
                    targetCohortIdentifier = "elfh-" + parsedParams["ugidweb"];
                }

                // Create or validate cohort
                MoodleCohort targetCohort = null;
                targetCohort = await cohortService.GetCohortByIdNumberAsync(targetCohortIdentifier);


                if (targetCohort != null)
                {
                    if (targetCohort == null)
                    {
                        result = $"ERROR: Target cohort '{targetCohortIdentifier}' not found in Moodle!\n";
                        await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", result);
                        return result;
                    }

                    await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", $"Mapping users to existing cohort '{targetCohort.Name}' (ID: {targetCohort.Id}).");
                    result += $"Mapping users to existing cohort '{targetCohort.Name}' (ID: {targetCohort.Id}).\n";
                }
                else
                {
                    // Create new cohort
                    result += await CreateMoodleCohort(elfhUserGroup);
                    createdNewCohort = true;
                }

                await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", $"Creating any missing users and assigning them to the cohort.");
                result += $"Creating any missing users and assigning them to the cohort.\n";

                int successCount = 0;
                int failCount = 0;

                var elfhUserList = await userRepository.SearchAsync(
                   new ElfhUserSearchModel() { SearchUserGroupId = elfhUserGroup.UserGroupId }
               );

                await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", $"There are {elfhUserList.Count()} elfh user(s) in the user group '{elfhUserGroup.UserGroupName}'.");
                result += $"There are {elfhUserList.Count()} elfh user(s) in the user group '{elfhUserGroup.UserGroupName}'.\n";


                foreach (var user in elfhUserList)
                {
                    // Create the user in Moodle
                    var createResult = await CreateElfhUser(user);
                    result += createResult;

                    // Assign the user to the cohort
                    var assignResult = await AssignUserToCohort(user.UserId, elfhUserGroupId, targetCohort?.Id, !createdNewCohort);
                    result += assignResult;

                    // Track success/failure - check for Moodle API error indicators
                    // Success: no "error" in response or contains "assigned" indication
                    // Moodle API returns error objects with "errorcode" and "message" keys
                    if (!assignResult.Contains("errorcode") && !assignResult.Contains("\"error\""))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                string summary = $"Migration Summary: Total users: {elfhUserList.Count()}, Successfully assigned: {successCount}";
                if (failCount > 0)
                    summary += $", Failed: {failCount}";

                await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", summary);
                result += $"\n{summary}\n";
            }
            else
            {
                result = "Parameter must be either 'id', 'username', 'ugid' or 'ugidweb'";
            }

            return result;
        }

        private async Task<string> AssignUserToCohort(int userName, int elfhUserGroupId, int? targetCohortId = null, bool isExistingCohort = false)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            string result = string.Empty;
            string additionalParameters = string.Empty;

            if (isExistingCohort && targetCohortId.HasValue)
            {
                // Assign to specific existing cohort by ID
                additionalParameters += "&members[0][cohorttype][type]=id";
                additionalParameters += $"&members[0][cohorttype][value]={targetCohortId}";
            }
            else
            {
                // Assign to cohort identified by idnumber (new or existing with elfh prefix)
                additionalParameters += "&members[0][cohorttype][type]=idnumber";
                additionalParameters += $"&members[0][cohorttype][value]=elfh-{elfhUserGroupId}";
            }

            additionalParameters += "&members[0][usertype][type]=username";
            additionalParameters += $"&members[0][usertype][value]={userName}";

            string url = $"&wsfunction=core_cohort_add_cohort_members{additionalParameters}";

            result = $"Assigning user '{userName}' to cohort.\n";
            await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", $"Assigning user '{userName}' to cohort.");
            result += await httpService.Get(url);

            return result;
        }

        private async Task<string> CreateMoodleCohort(ElfhUserGroup elfhUserGroup)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (elfhUserGroup == null)
            {
                return "User group not found!";
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "cohorts[0][categorytype][type]", "system" },
                    { "cohorts[0][categorytype][value]", "" },
                    { "cohorts[0][name]", elfhUserGroup.UserGroupName },
                    { "cohorts[0][idnumber]", $"elfh-{elfhUserGroup.UserGroupId}" },
                    { "cohorts[0][description]", elfhUserGroup.UserGroupDescription },
                    { "cohorts[0][descriptionformat]", "1" },
                    { "cohorts[0][visible]", "1" },
                    { "cohorts[0][theme]", "" }
                };

                string url = $"&wsfunction=core_cohort_create_cohorts";

                var result = await httpService.Post(url, parameters);
                string returnMessage = result.result;
                if (!string.IsNullOrEmpty(returnMessage))
                {
                    returnMessage += $"Cohort '{elfhUserGroup.UserGroupName}' created in Moodle\n";
                    await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Cohort '" + elfhUserGroup.UserGroupName + "' created in Moodle");

                }
                else
                {
                    returnMessage = $"FAILED to create Cohort '{elfhUserGroup.UserGroupName}' in Moodle\n";
                }
                return returnMessage;
            }
        }

        private async Task<string> CreateElfhUser(ElfhUser? elfhUser)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            string result = string.Empty;

            if (elfhUser == null)
            {
                return "User not found!";
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "users[0][createpassword]", "1" },
                    { "users[0][username]", elfhUser.UserId.ToString() },
                    { "users[0][email]", elfhUser.EmailAddress },
                    { "users[0][auth]", "oidc" },
                    { "users[0][firstname]", elfhUser.FirstName },
                    { "users[0][lastname]", elfhUser.LastName },
                    { "users[0][maildisplay]", "1" },
                    { "users[0][description]", "" },
                    { "users[0][department]", "" },
                    { "users[0][lang]", "en" },
                    { "users[0][firstnamephonetic]", "" },
                    { "users[0][lastnamephonetic]", "" },
                    { "users[0][middlename]", "" },
                    { "users[0][alternatename]", "" },
                    { "users[0][theme]", "" },
                    { "users[0][mailformat]", "1" }
                };

                string url = $"&wsfunction=core_user_create_users";

                result = $"\nCreating user '{elfhUser.UserName}'\n";
                await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Creating user '" + elfhUser.UserName + "' ");
                var returnResult = await httpService.Post(url, parameters);
                await hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "User '" + elfhUser.UserName + "' is created.");

                result += returnResult.result;

                return result;
            }
        }
    }
}