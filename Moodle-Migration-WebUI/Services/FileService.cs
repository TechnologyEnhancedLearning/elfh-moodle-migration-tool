using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moodle_Migration.Interfaces;
using System.Security.Principal;
using SimpleImpersonation;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR;
using Moodle_Migration_WebUI.Hubs;

namespace Moodle_Migration.Services
{
    public class FileService : IFileService
    {// Constants for WNetAddConnection2
        const int RESOURCETYPE_DISK = 0x00000001;
        private readonly IHttpContextAccessor _httpContextAccessor;
        [StructLayout(LayoutKind.Sequential)]
        public class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = RESOURCETYPE_DISK;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(
            NETRESOURCE netResource,
            string password,
            string username,
            int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(
            string name,
            int flags,
            bool force);
        private readonly IConfiguration _configuration;
        private readonly IHubContext<StatusHub> _hubContext;
        public FileService(IConfiguration configuration, IHubContext<StatusHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }
       
        public async Task<byte[]> DownloadFileAsync(string developmentId)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            //byte[] zipBytes = ZipFolderToBytes(folderPath); //This code can be used if Z drive is configured
            byte[] zipBytes = null;

            var networkPath = _configuration["ConnectionStrings:NetworkPath"];
            string? domain = _configuration["ConnectionStrings:Domain"];
            string? username = _configuration["ConnectionStrings:Username"];
            string? password = _configuration["ConnectionStrings:Password"];

            NETRESOURCE nr = new NETRESOURCE
            {
                lpRemoteName = networkPath
            };
            WNetCancelConnection2(networkPath, 0, true); // force disconnect
            await Task.Delay(500);
            // Add connection with credentials
            int result = WNetAddConnection2(nr, password, username, 0);
            if (result == 0)
            {
                await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Connection successful. Connected to content server.");
                Console.WriteLine("Connection successful!!! Connected to content server");

                try
                {
                    string folderPath = networkPath + "\\"+developmentId;
                    await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Getting scorm package from elfh content server.Please wait.");
                    zipBytes = ZipFolderToBytes(folderPath);
                    await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Scorm package retrieved from elfh content server.");
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Error accessing files. ");
                }
                
            }
            else
            {
                await _hubContext.Clients.User(currentUser).SendAsync("ReceiveStatus", "Error connecting to network share."+ result);
            }
            

            return zipBytes;
        }
       
        public static byte[] ZipFolderToBytes(string sourceFolderPath)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    AddDirectoryToZip(sourceFolderPath, archive, sourceFolderPath);
                }

                return memoryStream.ToArray();
            }
        }

        private static void AddDirectoryToZip(string sourcePath, ZipArchive archive, string basePath)
        {
            foreach (var filePath in Directory.GetFiles(sourcePath))
            {
                string entryName = Path.GetRelativePath(basePath, filePath);
                archive.CreateEntryFromFile(filePath, entryName);
            }

            foreach (var directory in Directory.GetDirectories(sourcePath))
            {
                AddDirectoryToZip(directory, archive, basePath);
            }
        }
    }
}
