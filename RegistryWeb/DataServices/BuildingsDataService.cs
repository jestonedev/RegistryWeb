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

namespace RegistryWeb.DataServices
{
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        private readonly string sqlDriver;
        private readonly string connString;
        private readonly string activityManagerPath;

        public BuildingsDataService(RegistryContext registryContext, IConfiguration config, IHttpContextAccessor httpContextAccessor) : base(registryContext)
        {
            sqlDriver = config.GetValue<string>("SqlDriver");
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");            
        }

        public override BuildingsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        public BuildingsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            BuildingsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public IQueryable<Building> GetQuery()
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdStateNavigation)
                .Where(b => b.Deleted == 0)
                .OrderBy(b => b.IdBuilding);
        }

        private IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = ObjectStateFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Building> ObjectStateFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdObjectState.HasValue && filterOptions.IdObjectState.Value != 0)
            {
                query = query.Where(b => b.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            }
            return query;
        }

        private IQueryable<Building> AddressFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => q.IdStreet.Equals(filterOptions.Address.Id));
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return query.Where(q => q.IdBuilding == id);
            }
            return query;
        }

        private IQueryable<Building> GetQueryOrder(IQueryable<Building> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdBuilding")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdBuilding);
                else
                    return query.OrderByDescending(b => b.IdBuilding);
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(b => b.IdStateNavigation.StateNeutral);
            }
            return query;
        }

        public IQueryable<Building> GetQueryPage(IQueryable<Building> query, PageOptions pageOptions)
        {
            return query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage);
        }

        public Building GetBuilding(int idBuilding)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .SingleOrDefault(b => b.IdBuilding == idBuilding);
        }

        public IEnumerable<Building> GetBuildings(List<int> ids)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Where(b => ids.Contains(b.IdBuilding));
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
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
    }
}
