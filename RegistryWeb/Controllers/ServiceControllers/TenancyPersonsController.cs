using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class TenancyPersonsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public TenancyPersonsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeletePerson(int? idPerson)
        {
            if (idPerson == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return -2;
            try
            {
                var person = registryContext.TenancyPersons
                    .FirstOrDefault(op => op.IdPerson == idPerson);
                person.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult SavePerson(TenancyPerson person)
        {
            if (person == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.TenancyWrite) && !securityService.HasPrivilege(Privileges.TenancyWriteEmailsOnly))
                return Json(new { Error = -2 });

            //Создать
            if (person.IdPerson == 0 && securityService.HasPrivilege(Privileges.TenancyWrite))
            {
                registryContext.TenancyPersons.Add(person);
                registryContext.SaveChanges();

                return Json(new { person.IdPerson });
            }
            //Обновить            
            if (securityService.HasPrivilege(Privileges.TenancyWrite))
            {
                registryContext.TenancyPersons.Update(person);
            } else
            if (securityService.HasPrivilege(Privileges.TenancyWriteEmailsOnly))
            {
                var personDb = registryContext.TenancyPersons.FirstOrDefault(tp => tp.IdPerson == person.IdPerson);
                personDb.Email = person.Email;
                personDb.PaymentAccount = person.PaymentAccount;
                registryContext.TenancyPersons.Update(personDb);
            }
            registryContext.SaveChanges();
            return Json(new { person.IdPerson });
        }

        public IActionResult AddDocumentIssuedBy(string documentIssuedByName)
        {
            if (string.IsNullOrEmpty(documentIssuedByName))
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);
            var duplicates = registryContext.DocumentsIssuedBy.FirstOrDefault(r => r.DocumentIssuedByName == documentIssuedByName);
            if (duplicates != null)
            {
                return Json(-3);
            }
            var documentIssuedBy = new DocumentIssuedBy { DocumentIssuedByName = documentIssuedByName };
            registryContext.DocumentsIssuedBy.Add(documentIssuedBy);
            registryContext.SaveChanges();
            return Json(documentIssuedBy.IdDocumentIssuedBy);
        }

        [HttpPost]
        public IActionResult UpdateExcludeDate(int? idPerson, DateTime excludeDate)
        {
            if (idPerson == null || idPerson == 0)
            {
                return Json(new
                {
                    Code = -1,
                    Text = "Не удалось найти участника найма"
                });
            }
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
            {
                return Json(new
                {
                    Code = -1,
                    Text = "У вас нет прав на редактирование найма жилья"
                });
            }
            var person = registryContext.TenancyPersons.FirstOrDefault(p => p.IdPerson == idPerson);
            person.ExcludeDate = excludeDate;
            registryContext.SaveChanges();
            return Json(new
            {
                Code = 0
            });
        }

        [HttpPost]
        public IActionResult UpdateIdKinship(int? idPerson, int idKinship)
        {
            if (idPerson == null || idPerson == 0)
            {
                return Json(new
                {
                    Code = -1,
                    Text = "Не удалось найти участника найма"
                });
            }
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
            {
                return Json(new
                {
                    Code = -1,
                    Text = "У вас нет прав на редактирование найма жилья"
                });
            }
            var person = registryContext.TenancyPersons.FirstOrDefault(p => p.IdPerson == idPerson);
            person.IdKinship = idKinship;
            registryContext.SaveChanges();
            return Json(new
            {
                Code = 0
            });
        }
    }
}