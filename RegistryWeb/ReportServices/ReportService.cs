using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Diagnostics;

namespace RegistryWeb.ReportServices
{
    public class ReportService
    {
        private readonly string sqlDriver;
        private readonly string connString;
        private readonly string activityManagerPath;

        public ReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            sqlDriver = config.GetValue<string>("SqlDriver");
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");            
        }
        
        public byte[] Forma1(List<int> ids)
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\forma1.xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "forma1" + Guid.NewGuid().ToString() + ".docx");

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
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                File.Delete(fileName);
                return file;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        public byte[] Forma2(List<int> ids)
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\forma2.xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "forma2" + Guid.NewGuid().ToString() + ".docx");

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
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                File.Delete(fileName);
                return file;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }
    }
}
