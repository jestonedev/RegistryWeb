﻿using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.DataHelpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryDb.Models.SqlViews;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace RegistryWeb.DataServices
{
    public class PrivRealtorService
    {
        private readonly RegistryContext rc;

        public PrivRealtorService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public PrivRealtorVM<PrivRealtor> GetViewModel(PageOptions pageOptions)
        {
            var viewModel = new PrivRealtorVM<PrivRealtor>
            {
                PageOptions = pageOptions ?? new PageOptions()
            };
            var query = rc.PrivRealtors.AsQueryable();
            viewModel.PageOptions.TotalRows = query.Count();
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.PrivRealtors = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public List<PrivRealtor> GetQueryPage(IQueryable<PrivRealtor> query, PageOptions pageOptions)
        {
            var result = query;
            result = result.Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage);
            result = result.Take(pageOptions.SizePage);
            return result.ToList();
        }

        public PrivRealtorVM<PrivRealtor> GetPrivRealtorView(PrivRealtor privRealtor)
        {
            var privRealtorVM = new PrivRealtorVM<PrivRealtor>()
            {
                PrivRealtor = privRealtor
            };
            return privRealtorVM;
        }

        internal void Create(PrivRealtor privRealtor)
        {
            rc.PrivRealtors.Add(privRealtor);
            rc.SaveChanges();
        }

        internal void Edit(PrivRealtor privRealtor)
        {
            rc.PrivRealtors.Update(privRealtor);
            rc.SaveChanges();
        }

        internal void Delete(PrivRealtor privRealtor)
        {
            privRealtor.Deleted = 1;
            rc.SaveChanges();
        }

    }
}
