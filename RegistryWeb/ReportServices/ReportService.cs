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
        
        private string GenerateMultiReport(List<int> ids, string name)
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\" + name + ".xml";
                var fileNameReport = name + Guid.NewGuid().ToString() + ".docx";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileNameReport);

                var fileName = Path.GetTempFileName();
                using (var sw = new StreamWriter(fileName))
                    sw.Write(ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y));

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                    "\" idsTmpFile=\"" + fileName + "\" connectionString=\"Driver={" + sqlDriver + "};" + connString + "\"";
                logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                File.Delete(fileName);
                return fileNameReport;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
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

        private byte[] Report(int id, string name)
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\" + name + ".xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", name + Guid.NewGuid().ToString() + ".docx");

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                    "\" id=\"" + id + "\" connectionString=\"Driver={" + sqlDriver + "};" + connString + "\"";
                logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                return file;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        public byte[] Forma1(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "forma1");
            return DownloadFile(fileNameReport);
        }
        
        public string Forma2Ajax(List<int> ids)
        {
            return GenerateMultiReport(ids, "forma2");
        }

        public byte[] Forma2(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "forma2");
            return DownloadFile(fileNameReport);
        }

        public string Forma3Ajax(List<int> ids)
        {
            return GenerateMultiReport(ids, "forma3");
        }

        public byte[] Forma3(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "forma3");
            return DownloadFile(fileNameReport);
        }

        private string getExtentionFile(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.') + 1, fileName.Length - fileName.LastIndexOf('.') - 1).ToLower();
        }

        private string getMIMEType(string fileName)
        {
            switch (getExtentionFile(fileName))
            {
                case "pdf":
                    return "application/pdf";
                case "jpeg":
                case "jpg":
                    return "application/jpeg";
                case "png":
                    return "application/png";
                case "tiff":
                    return "application/tiff";
                case "odt":
                    return "application/vnd.oasis.opendocument.text";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                default:
                    return "";
            }
        }

        public (byte[], string) GetFileContentsAndMIMETypeFromRepository(string fileName, ActFileTypes actFileType)
        {
            try
            {
                var path = Path.Combine(attachmentsPath, actFileType.ToString() + 's', fileName);
                var type = getMIMEType(fileName);
                var result = File.ReadAllBytes(path);
                return (result, type);
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
                var fileName = Guid.NewGuid().ToString() + '.' + getExtentionFile(file.FileName);
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
