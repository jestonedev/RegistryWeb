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
using System.IO.Compression;

namespace RegistryWeb.ReportServices
{
    public class ReportService
    {
        private readonly string sqlDriver;
        private readonly string connString;
        protected readonly string activityManagerPath;
        protected readonly string invoiceGeneratorPath;
        private readonly string attachmentsPath;

        public ReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            sqlDriver = config.GetValue<string>("SqlDriver");
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");
            invoiceGeneratorPath = config.GetValue<string>("RegistryInvoiceGeneratorPath");
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

        protected string GenerateMultiFileReport(Dictionary<string, object> arguments, string config)
        {
            var logStr = new StringBuilder();
            try
            {
                var configXml = activityManagerPath + "templates\\" + config + ".xml";
                var configParts = config.Split('\\');
                var directoryName = configParts[configParts.Length - 1] + Guid.NewGuid().ToString();
                var destDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", directoryName);
                Directory.CreateDirectory(destDirectory);
                
                arguments.Add("config", configXml);
                arguments.Add("destDirectoryName", destDirectory);
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

                var destZipFile = destDirectory + ".zip";
                ZipFile.CreateFromDirectory(destDirectory, destZipFile);
                Directory.Delete(destDirectory, true);
                return destZipFile;
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

        private static string GetArgumentsForGenerator(Dictionary<string, object> arguments)
        {
            var argumentsString = "";
            foreach (var argument in arguments)
                argumentsString += string.Format(CultureInfo.InvariantCulture, " {0}=\"{1}\"",
                    argument.Key,
                    argument.Value == null ? "" : argument.Value.ToString());
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

        protected int GenerateInvoice(Dictionary<string, object> arguments)
        {
            var logStr = new StringBuilder();
            //var invoiceGeneratorPath = @"D:\Projects\registryinvoicegenerator\RegistryInvoiceGenerator\RegistryInvoiceGenerator\bin\Debug\netcoreapp2.2\";
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "dotnet";
                    p.StartInfo.WorkingDirectory = invoiceGeneratorPath;
                    p.StartInfo.Arguments = "\"" + Path.Combine(invoiceGeneratorPath, "RegistryInvoiceGenerator.dll") + "\"" + GetArgumentsForGenerator(arguments);

                    logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                    p.Start();
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        protected Dictionary<Dictionary<string, object>, int> GenerateInvoices(List<Dictionary<string, object>> invoices)
        {
            var runnedInvoices = new Dictionary<Dictionary<string, object>, Process>();
            var runnInvoiceResults = new Dictionary<Dictionary<string, object>, int>();
            var dic = new Dictionary<int, IEnumerable<string>>();
            var invoiceGeneratorPath = @"D:\Projects\registryinvoicegenerator\RegistryInvoiceGenerator\RegistryInvoiceGenerator\bin\Debug\netcoreapp2.2\";

            foreach (var invoice in invoices)
            {
                var p = new Process();
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.WorkingDirectory = invoiceGeneratorPath;
                p.StartInfo.Arguments = "\"" + Path.Combine(invoiceGeneratorPath, "RegistryInvoiceGenerator.dll") + "\"" + GetArgumentsForGenerator(invoice);                    
                p.Start();

                runnedInvoices.Add(invoice, p);                

                if (runnedInvoices.Count >= 100 || runnedInvoices.Count==invoices.Count)
                {
                    foreach(var runnedInvoice in runnedInvoices)
                    {
                        if (!runnedInvoice.Value.HasExited)
                            runnedInvoice.Value.WaitForExit();
                        runnInvoiceResults.Add(runnedInvoice.Key, runnedInvoice.Value.ExitCode);
                        runnedInvoice.Value.Dispose();

                    }
                    runnedInvoices.Clear();
                }
            }
            return runnInvoiceResults;
        }
    }
}
