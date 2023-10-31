using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;

namespace RegistryWeb.ViewComponents
{
    public class AttachmentFilesComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public AttachmentFilesComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id, AddressTypes type, ActionTypeEnum action)
        {
            ViewBag.Id = id;
            IEnumerable<AttachmentFileVM> model = null;
            if (type == AddressTypes.Building)
            {
                model = GetBuildingAttachmentFiles(id);
            }
            if (type == AddressTypes.Premise)
            {
                model = GetPremiseAttachmentFiles(id);
            }
            ViewBag.AddressType = type;
            ViewBag.Action = action;

            return View("AttachmentFiles", model);
        }

        private IEnumerable<AttachmentFileVM> GetBuildingAttachmentFiles(int idBuilding)
        {
            var afs = registryContext.BuildingAttachmentFilesAssoc
                    .Include(oba => oba.ObjectAttachmentFileNavigation)
                    .Where(oba => oba.IdBuilding == idBuilding)
                    .Select(oba => oba.ObjectAttachmentFileNavigation);
            var r =
                from af in afs
                select new AttachmentFileVM(af, AddressTypes.Building);
            return r;
        }

        private IEnumerable<AttachmentFileVM> GetPremiseAttachmentFiles(int idPremise)
        {
            return null;
        }
    }
}
