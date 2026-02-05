using Microsoft.IdentityModel.Tokens;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;
using System;
using System.Text.Json;

namespace Moodle_Migration.Services
{
    public class UserService(IHttpService httpService, IUserRepository userRepository, IUserGroupRepository userGroupRepository) : IUserService
    {
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
                        return($"Parameters must be in the format 'key=value' ({parameters[i]})");
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
            string result = string.Empty;

            if (parameters.Length == 0)
            {
                return "No user data specified!";
            }
            if (parameters.Length > 1)
            {
                //Console.WriteLine("A single parameter in the format 'parameter=value' is required.");
                //Console.WriteLine("(The 'parameter' can be 'id', 'username' or 'ugid')");
                return "A single parameter in the format 'parameter=value' is required. \n (The 'parameter' can be 'id', 'username' or 'ugid')";
            }
            if (!parameters[0].Contains("="))
            {
                return "Parameters must be in the format 'parameter=value')";
            }

            var parameter = parameters[0].Split('=')[0];
            var value = parameters[0].Split('=')[1];
            ElfhUser? elfhUser = null;

            switch (parameter)
            {
                case "id":
                    int elfhUserId = 0;
                    Int32.TryParse(value, out elfhUserId);

                    if (elfhUserId == 0)
                    {
                        return "Invalid user ID!";
                    }
                    elfhUser = await userRepository.GetByIdAsync(elfhUserId);
                    result = await CreateElfhUser(elfhUser);
                    break;
                case "username":

                    if (value.IsNullOrEmpty())
                    {
                        return "Empty username!";
                    }
                    elfhUser = await userRepository.GetByUserNameAsync(value);
                    result = await CreateElfhUser(elfhUser);
                    break;
                case "ugidweb":
                    int elfhUserGroupId = 0;
                    Int32.TryParse(value, out elfhUserGroupId);

                    if (elfhUserGroupId == 0)
                    {
                        return "Invalid user group ID!";
                    }

                    var elfhUserGroup = await userGroupRepository.GetByIdAsync(elfhUserGroupId);

                    if (elfhUserGroup == null)
                    {
                        return "User group not found!";
                    }
                    else
                    {

                        result = await CreateMoodleCohort(elfhUserGroup);

                        var elfhUserList = await userRepository.SearchAsync(
                            new ElfhUserSearchModel() { SearchUserGroupId = elfhUserGroup.UserGroupId }
                            );

                        result += $"There are {elfhUserList.Count()} elfh user(s) in the user group '{elfhUserGroup.UserGroupName}'.\n ";
                        result += $"This will create any missing users and assign them to the '{elfhUserGroup.UserGroupName}' user group.\n ";

                        foreach (var user in elfhUserList)
                        {
                            // Create the user in Moodle
                            result = result + await CreateElfhUser(user);

                            // Assign the user to the user group in Moodle
                            result = result + await AssignUserToCohort(user.UserId, elfhUserGroupId);
                        }

                    }
                    break;
                case "ugid":
                    elfhUserGroupId = 0;
                    Int32.TryParse(value, out elfhUserGroupId);

                    if (elfhUserGroupId == 0)
                    {
                        return "Invalid user group ID!";
                    }

                    elfhUserGroup = await userGroupRepository.GetByIdAsync(elfhUserGroupId);

                    if (elfhUserGroup == null)
                    {
                        return "User group not found!";
                    }
                    else
                    {

                        result = await CreateMoodleCohort(elfhUserGroup);

                        var elfhUserList = await userRepository.SearchAsync(
                            new ElfhUserSearchModel() { SearchUserGroupId = elfhUserGroup.UserGroupId }
                            );

                        Console.WriteLine($"There are {elfhUserList.Count()} elfh user(s) in the user group '{elfhUserGroup.UserGroupName}'");
                        Console.WriteLine($"Would you like to create any missing users and asign them to the '{elfhUserGroup.UserGroupName}' user group?");
                        Console.WriteLine("Press 'Y' to continue or any other key to exit.");

                        ConsoleKeyInfo keyInfo = Console.ReadKey();
                        Console.WriteLine();
                        if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
                        {
                            foreach (var user in elfhUserList)
                            {
                                // Create the user in Moodle
                                result +=  await CreateElfhUser(user);

                                // Assign the user to the user group in Moodle
                                result = result + await AssignUserToCohort(user.UserId, elfhUserGroupId);
                            }
                        }
                    }
                    break;
                default:
                    result = "Parameter must be either 'id', 'username' or 'ugid'";
                    break;
            }

            return result;
        }

        private async Task<string> AssignUserToCohort(int userName, int elfhUserGroupId)
        {
            string result = string.Empty;
            string additionalParameters = string.Empty;
            additionalParameters += "&members[0][cohorttype][type]=idnumber";
            additionalParameters += $"&members[0][cohorttype][value]=elfh-{elfhUserGroupId}";
            additionalParameters += "&members[0][usertype][type]=username";
            additionalParameters += $"&members[0][usertype][value]={userName}";

            string url = $"&wsfunction=core_cohort_add_cohort_members{additionalParameters}";

            result = $"Assigning '{userName}' to cohort.";
            result +=  await httpService.Get(url);

            return result;
        }

        private async Task<string> CreateMoodleCohort(ElfhUserGroup elfhUserGroup)
        {
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
                var returnResult = await httpService.Post(url, parameters);
                result += returnResult.result;

                return result;
            }
        }
    }
}