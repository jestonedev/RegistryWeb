using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader;
using RegistryPaymentsLoader.TffFileLoaders;
using RegistryPaymentsLoader.TffStrings;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;

namespace RegistryWeb.Controllers
{
    public class KumiPaymentsController : ListController<KumiPaymentsDataService, KumiPaymentsFilter>
    {
        public KumiPaymentsController(KumiPaymentsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadPayments(List<IFormFile> files)
        {
            var tffStrings = new List<TffString>();
            var kumiPaymentGroupFiles = new List<KumiPaymentGroupFile>();
            foreach (var file in files)
            {
                try
                {
                    var stream = file.OpenReadStream();
                    var tffFileLoader = TffFileLoaderFactory.CreateFileLoader(stream);
                    if (tffFileLoader == null) continue;
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    tffStrings.AddRange(tffFileLoader.Load(stream));
                    stream.Close();
                    kumiPaymentGroupFiles.Add(new KumiPaymentGroupFile
                    {
                        FileName = file.FileName,
                        FileVersion = tffFileLoader.Version
                    });
                }
                catch (BDFormatException e)
                {
                    return Error(string.Format("Файл {0}. "+e.Message, file.FileName));
                }
                catch (Exception e)
                {
                    return Error(e.Message);
                }
            }
            var errorModel = dataService.UploadInfoFromTff(tffStrings, kumiPaymentGroupFiles);
            return View("UploadPaymentsResult", errorModel);
        }
    }
}