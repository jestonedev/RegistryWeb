using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryWeb.Extensions;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    public class SessionController<F> : RegistryBaseController where F : FilterOptions
    {
        protected string nameFilteredIdsDict;
        protected string nameIds;
        protected string nameMultimaster;

        public SessionController()
        {
        }

        [HttpPost]
        public List<int> GetSessionIds()
        {
            if (HttpContext.Session.Keys.Contains(nameIds))
                return HttpContext.Session.Get<List<int>>(nameIds);
            return new List<int>();
        }

        public IActionResult ClearSessionIds()
        {
            HttpContext.Session.Remove(nameIds);
            ViewBag.Count = 0;
            return RedirectToAction(nameMultimaster);
        }

        public IActionResult RemoveSessionId(int id)
        {
            var ids = GetSessionIds();
            ids.Remove(id);
            ViewBag.Count = ids.Count();
            HttpContext.Session.Set(nameIds, ids);
            return RedirectToAction(nameMultimaster);
        }

        [HttpPost]
        public void CheckIdToSession(int id, bool isCheck)
        {
            List<int> ids = GetSessionIds();

            if (isCheck)
                ids.Add(id);
            else if (ids.Any())
                ids.Remove(id);

            HttpContext.Session.Set(nameIds, ids);
        }

        public void AddSearchIdsToSession(FilterOptions filterOptions, List<int> filteredIds)
        {
            var filteredIdsDict = new Dictionary<string, List<int>>();

            if (HttpContext.Session.Keys.Contains(nameFilteredIdsDict))
                filteredIdsDict = HttpContext.Session.Get<Dictionary<string, List<int>>>(nameFilteredIdsDict);

            var filterOptionsSerialized = JsonConvert.SerializeObject(filterOptions).ToString();

            if (filteredIdsDict.Keys.Contains(filterOptionsSerialized))
                filteredIdsDict[filterOptionsSerialized] = filteredIds;
            else filteredIdsDict.Add(filterOptionsSerialized, filteredIds);

            HttpContext.Session.Set(nameFilteredIdsDict, filteredIdsDict);
        }

        [HttpPost]
        public IActionResult AddSelectedAndFilteredIdsToSession(F filterOptions)
        {
            if (!HttpContext.Session.Keys.Contains(nameFilteredIdsDict))
                return Json(0);

            var filteredIdsDict = HttpContext.Session.Get<Dictionary<string, List<int>>>(nameFilteredIdsDict);
            var filterOptionsSerialized = JsonConvert.SerializeObject(filterOptions).ToString();

            if (filteredIdsDict.Keys.Contains(filterOptionsSerialized))
            {
                List<int> filterOptionsIds = filteredIdsDict[filterOptionsSerialized];
                List<int> ids = GetSessionIds();
                ids.AddRange(filterOptionsIds);
                ids = ids.Distinct().ToList();
                HttpContext.Session.Set(nameIds, ids);
                return Json(0);
            }
            return Json(-1);
        }
    }
}