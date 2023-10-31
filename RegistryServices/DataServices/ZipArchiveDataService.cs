using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;

namespace RegistryWeb.DataServices
{

    public class ZipArchiveDataService
    {
        private readonly IConfiguration config;

        public ZipArchiveDataService(IConfiguration config)
        {
            this.config = config;
        }

        public List<string> UnpackRecursive(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            var zPath = config.GetValue<string>("ExtreactorPath");
            if (!File.Exists(zPath)) return null;
            var destPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);

            try
            {
                ProcessStartInfo pi = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = zPath,
                    Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", fileName, destPath)
                };
                Process process = Process.Start(pi);
                process.WaitForExit();

                File.Delete(fileName);


                var archives = Directory.GetFiles(destPath, "*.zip", SearchOption.AllDirectories).Union(Directory.GetFiles(destPath, "*.7z"));

                var files = Directory.GetFiles(destPath, "*.*", SearchOption.AllDirectories).ToList();
                files = files.Except(archives).ToList();
                foreach (var archive in archives)
                {
                    var subFiles = UnpackRecursive(archive);
                    files.AddRange(subFiles);
                }

                return files;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
