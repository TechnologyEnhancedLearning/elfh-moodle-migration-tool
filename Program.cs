using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moodle_Migration;
using Moodle_Migration.Interfaces;
using Moodle_Migration.Repositories;
using Moodle_Migration.Services;
using System.Data;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        // Set up dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Get the main application service and run it
        var app = serviceProvider.GetService<App>();
        await app.Run(args);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        services.AddSingleton(configuration);

        // Register application entry point
        services.AddTransient<App>();

        // Register the Moodle HttpClient
        services.AddHttpClient("MoodleClient", client =>
        {
            client.BaseAddress = new Uri(configuration["MoodleApi:BaseUrl"]);
        });

        services.AddTransient<IDbConnection>(db => new SqlConnection(configuration.GetConnectionString("ElfhHubDbConnection")));

        // Register data services
        services.AddTransient<IHttpService, HttpService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ICourseService, CourseService>();
        services.AddTransient<ICategoryService, CategoryService>();

        // Register data repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserGroupRepository, UserGroupRepository>();
    }
}
