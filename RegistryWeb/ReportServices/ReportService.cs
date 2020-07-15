using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Diagnostics;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
using System.Globalization;

namespace RegistryWeb.ReportServices
{
    public class ReportService
    {
        private readonly string sqlDriver;
        private readonly string connString;
        private readonly string activityManagerPath;
        private readonly string attachmentsPath;

        public ReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            sqlDriver = config.GetValue<string>("SqlDriver");
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");    
            attachmentsPath = config.GetValue<string>("AttachmentsPath");
        }

        protected string GenerateReport(Dictionary<string, object> arguments, string config)
        {
            var logStr = new StringBuilder();
            try
            {
                var configXml = activityManagerPath + "templates\\" + config + ".xml";
                var configParts = config.Split('\\');
                var fileNameReport = configParts[configParts.Length - 1] + Guid.NewGuid().ToString() + ".docx";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileNameReport);

                arguments.Add("config", configXml);
                arguments.Add("destFileName", destFileName);
                arguments.Add("force-move-to", destFileName);
                arguments.Add("connectionString", "Driver={" + sqlDriver + "};" + connString);

                using (var p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                    p.StartInfo.Arguments = GetArguments(arguments);
                    logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                }

                return fileNameReport;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        private static string GetArguments(Dictionary<string, object> arguments)
        {
            var argumentsString = "";
            foreach (var argument in arguments)
                argumentsString += string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\" ",
                    argument.Key.Replace("\"", "\\\""),
                    argument.Value == null ? "" : argument.Value.ToString().Replace("\"", "\\\""));
            return argumentsString;
        }

        public byte[] DownloadFile(string fileName)
        {
            try
            {
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string getExtentionFile(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.') + 1, fileName.Length - fileName.LastIndexOf('.') - 1).ToLower();
        }


        public byte[] GetFileContentsFromRepository(string fileName, ActFileTypes actFileType)
        {
            try
            {
                var path = Path.Combine(attachmentsPath, actFileType.ToString() + 's', fileName);
                var file = File.ReadAllBytes(path);
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SaveFormFileToRepository(IFormFile file, ActFileTypes actFileType)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + new FileInfo(file.FileName).Extension;
                var path = Path.Combine(attachmentsPath, actFileType.ToString() + 's', fileName);
                var fileStream = new FileStream(path, FileMode.Create);
                file.CopyTo(fileStream);
                fileStream.Dispose();
                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteFileToRepository(string fileName, ActFileTypes actFileType)
        {
            try
            {
                var path = Path.Combine(attachmentsPath, actFileType.ToString() + 's', fileName);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
