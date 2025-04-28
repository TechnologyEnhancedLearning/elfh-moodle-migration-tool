using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using Moodle_Migration.Interfaces;
using System.Security.Principal;
using SimpleImpersonation;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Moodle_Migration.Services
{
    public class FileService : IFileService
    {// Constants for WNetAddConnection2
        const int RESOURCETYPE_DISK = 0x00000001;

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
        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;

        }
       
        public async Task<byte[]> DownloadFileAsync(string developmentId)
        {

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
            // Add connection with credentials
            int result = WNetAddConnection2(nr, password, username, 0);

            if (result == 0)
            {
                Console.WriteLine("Connection successful!!! Connected to content server");

                try
                {
                    string folderPath = networkPath + "\\"+developmentId;
                    Console.WriteLine("Getting scorm file from elfh content server!!!Please wait");
                    zipBytes = ZipFolderToBytes(folderPath);
                    Console.WriteLine("File fetched from content server");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error accessing files: " + ex.Message);
                }
                finally
                {
                    // Optional: Disconnect
                    WNetCancelConnection2(networkPath, 0, true);
                }
            }
            else
            {
                Console.WriteLine("Error connecting to network share. Error code: " + result);
            }
            

            return zipBytes;
        }
        static async Task DownloadDirectoryRecursive(ShareDirectoryClient remoteDir, string localPath)
        {
            Directory.CreateDirectory(localPath);

            await foreach (ShareFileItem item in remoteDir.GetFilesAndDirectoriesAsync())
            {
                if (item.IsDirectory)
                {
                    var subDirClient = remoteDir.GetSubdirectoryClient(item.Name);
                    string subDirPath = Path.Combine(localPath, item.Name);
                    await DownloadDirectoryRecursive(subDirClient, subDirPath);
                }
                else
                {
                    var fileClient = remoteDir.GetFileClient(item.Name);
                    string localFilePath = Path.Combine(localPath, item.Name);

                    ShareFileDownloadInfo download = await fileClient.DownloadAsync();

                    using (FileStream stream = File.OpenWrite(localFilePath))
                    {
                        await download.Content.CopyToAsync(stream);
                        Console.WriteLine($"Downloaded: {localFilePath}");
                    }
                }
            }
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
