using Microsoft.IdentityModel.Tokens;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Models;

namespace Moodle_Migration.Services
{
    public class UserService(IHttpService httpService, IUserRepository userRepository, IUserGroupRepository userGroupRepository) : IUserService
    {
        public async Task ProcessUser(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("No user option specified!");
                return;
            }
            args = args.Skip(1).ToArray();
            var parameters = args.Skip(1).ToArray();
            switch (args[0])
            {
                case "-d":
                case "--details":
                    await GetUsers(parameters);
                    break;
                case "-c":
                case "--create":
                    await CreateUsers(parameters);
                    break;
                default:
                    Console.WriteLine("Invalid user option!");
                    break;
            }
        }

        private async Task GetUsers(string[] parameters)
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
                            throw new ArgumentException($"Parameters must be in the format 'key=value' ({parameters[i]})");
                        }
                    var key = parameters[i].Split('=')[0];
                    var value = parameters[i].Split('=')[1];
                    additionalParameters += $"&criteria[{i}][key]={key}&criteria[{i}][value]={value}";
                }
            }

            string url = $"&wsfunction=core_user_get_users{additionalParameters}";
            await httpService.Get(url);
        }

        private async Task CreateUsers(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine("No user data specified!");
                return;
            }
            if (parameters.Length > 1)
            {
                Console.WriteLine("A single parameter in the format 'parameter=value' is required.");
                Console.WriteLine("(The 'parameter' can be 'id', 'username' or 'ugid')");
                return;
            }
            if (!parameters[0].Contains("="))
            {
                Console.WriteLine("Parameters must be in the format 'parameter=value')");
                return;
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
                        Console.WriteLine("Invalid user ID!");
                        return;
                    }
                    elfhUser = await userRepository.GetByIdAsync(elfhUserId);
                    await CreateElfhUser(elfhUser);
                    break;
                case "username":

                    if (value.IsNullOrEmpty())
                    {
                        Console.WriteLine("Empty username!");
                        return;
                    }
                    elfhUser = await userRepository.GetByUserNameAsync(value);
                    await CreateElfhUser(elfhUser);
                    break;
                case "ugid":
                    int elfhUserGroupId = 0;
                    Int32.TryParse(value, out elfhUserGroupId);

                    if (elfhUserGroupId == 0)
                    {
                        Console.WriteLine("Invalid user group ID!");
                        return;
                    }

                    var elfhUserGroup = await userGroupRepository.GetByIdAsync(elfhUserGroupId);

                    if (elfhUserGroup == null)
                    {
                        Console.WriteLine("User group not found!");
                    }
                    else
                    {

                        await CreateMoodleCohort(elfhUserGroup);
                        
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
                                await CreateElfhUser(user);

                                // Assign the user to the user group in Moodle
                                await AssignUserToCohort(user.UserName, elfhUserGroupId);
                            }
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Parameter must be either 'id', 'username' or 'ugid'");
                    break;
            }
        }

        private async Task AssignUserToCohort(string userName, int elfhUserGroupId)
        {
            string additionalParameters = string.Empty;
            additionalParameters += "&members[0][cohorttype][type]=idnumber";
            additionalParameters += $"&members[0][cohorttype][value]=elfh-{elfhUserGroupId}";
            additionalParameters += "&members[0][usertype][type]=username";
            additionalParameters += $"&members[0][usertype][value]={userName}";

            string url = $"&wsfunction=core_cohort_add_cohort_members{additionalParameters}";

            Console.WriteLine($"Assigning '{userName}' to cohort.");
            await httpService.Get(url);
        }

        private async Task CreateMoodleCohort(ElfhUserGroup elfhUserGroup)
        {
            string additionalParameters = string.Empty;
            additionalParameters += "&cohorts[0][categorytype][type]=system";
            additionalParameters += "&cohorts[0][categorytype][value]=";
            additionalParameters += $"&cohorts[0][name]={elfhUserGroup.UserGroupName}";
            additionalParameters += $"&cohorts[0][idnumber]=elfh-{elfhUserGroup.UserGroupId}";
            additionalParameters += $"&cohorts[0][description]={elfhUserGroup.UserGroupDescription}";
            additionalParameters += "&cohorts[0][descriptionformat]=1";
            additionalParameters += "&cohorts[0][visible]=1";
            additionalParameters += "&cohorts[0][theme]=";
            //additionalParameters += "&cohorts[0][customfields][0][shortname]=";
            //additionalParameters += "&cohorts[0][customfields][0][value]=";

            Console.WriteLine($"Creating cohort '{elfhUserGroup.UserGroupName}'");
            string url = $"&wsfunction=core_cohort_create_cohorts{additionalParameters}";

            await httpService.Get(url);
        }

        private async Task CreateElfhUser(ElfhUser? elfhUser)
        {
            if (elfhUser == null)
            {
                Console.WriteLine("User not found!");
            }
            else
            {
                string additionalParameters = string.Empty;
                additionalParameters += "&users[0][createpassword]=1";
                additionalParameters += $"&users[0][username]={elfhUser.UserName.ToLower()}";
                additionalParameters += $"&users[0][email]={elfhUser.EmailAddress}";
                additionalParameters += "&users[0][auth]=manual";
                additionalParameters += $"&users[0][firstname]={elfhUser.FirstName}";
                additionalParameters += $"&users[0][lastname]={elfhUser.LastName}";
                additionalParameters += "&users[0][maildisplay]=1";
                additionalParameters += "&users[0][description]=";
                additionalParameters += "&users[0][department]=";
                additionalParameters += "&users[0][lang]=en";
                additionalParameters += "&users[0][theme]=";
                additionalParameters += "&users[0][mailformat]=1";

                string url = $"&wsfunction=core_user_create_users{additionalParameters}";

                Console.WriteLine($"Creating user '{elfhUser.UserName}'");
                await httpService.Get(url);
            }
        }
    }
}