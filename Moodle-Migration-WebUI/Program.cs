using Moodle_Migration.Interfaces;
using Moodle_Migration.Services;
using Moodle_Migration_WebUI.Services;
using Moodle_Migration.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;
using Moodle_Migration_WebUI.Interfaces;
using Moodle_Migration_WebUI;
using Moodle_Migration_WebUI.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Moodle_Migration_WebUI.Hubs;
using Moodle_Migration_WebUI.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Prevent JavaScript access to the cookie
    options.Cookie.IsEssential = true; // Ensure the cookie is always available
});

// Add services to the container.
builder.Services.AddControllersWithViews();
var configuration = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
           .Build();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Command/Index"; // Set your login path
    });

// Set up dependency injection
var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection, configuration);

void ConfigureServices(ServiceCollection serviceCollection, IConfigurationRoot configuration)
{
    // Register configuration
    builder.Services.AddSingleton(configuration);
    builder.Services.AddSignalR();
    builder.Services.AddAuthorization();
    builder.Services.AddSession();
    builder.Services.AddHttpContextAccessor();
    // Register the Moodle HttpClient
    builder.Services.AddHttpClient("MoodleClient", client =>
    {
        client.BaseAddress = new Uri(configuration["MoodleApi:BaseUrl"]);
    });

    builder.Services.AddTransient<IDbConnection>(db => new SqlConnection(configuration.GetConnectionString("ElfhHubDbConnection")));
    builder.Services.AddDbContext<LoggingDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MigrationToolLog")));

   builder.Services.Configure<LoginCredentials>(configuration.GetSection("LoginCredentials"));

    // Register data services
    builder.Services.AddTransient<IHttpService, HttpService>();
    builder.Services.AddTransient<IFileService, FileService>();
    builder.Services.AddTransient<IUserService, UserService>();
    builder.Services.AddTransient<ICourseService, CourseService>();
    builder.Services.AddTransient<ICategoryService, CategoryService>();
    
    // Register data repositories
    builder.Services.AddTransient<IUserRepository, UserRepository>();
    builder.Services.AddTransient<IUserGroupRepository, UserGroupRepository>();
    builder.Services.AddTransient<IComponentRepository, ComponentRepository>();
    builder.Services.AddTransient<ILoggingRepository, LoggingRepository>();

}

builder.Services.AddTransient<ICommandProcessor, CommandProcessor>();
builder.Services.AddTransient<ICommandService, CommandService>();

var app = builder.Build();

app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.ToString().ToLower();

    // Allow access to the login/ logout endpoints without authentication
    if (path.StartsWith("/home") || path.StartsWith("/favicon.ico"))
    {
        await next();
        return;
    }

    // Redirect if not logged in
    if (context.Session.GetString("IsLoggedIn") != "true")
    {
        context.Response.Redirect("/Home/Login");
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");
app.MapHub<StatusHub>("/statushub");
app.Run();
