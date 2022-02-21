using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using System.Linq;
using System.Collections.Generic;
using System;


namespace RegistryWeb.DataServices
{
    public class DocumentIssuerService
    {
        private readonly RegistryContext rc;

        public DocumentIssuerService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public DocumentIssueVM<DocumentIssuedBy> GetViewModel(PageOptions pageOptions)
        {
            var viewModel = new DocumentIssueVM<DocumentIssuedBy>
            {
                PageOptions = pageOptions ?? new PageOptions()
            };
            var query = rc.DocumentsIssuedBy.AsQueryable();
            viewModel.PageOptions.TotalRows = query.Count();
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.DocumentIssues = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public List<DocumentIssuedBy> GetQueryPage(IQueryable<DocumentIssuedBy> query, PageOptions pageOptions)
        {
            var result = query;
            result = result.Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage);
            result = result.Take(pageOptions.SizePage);
            return result.ToList();
        }

        public DocumentIssueVM<DocumentIssuedBy> GetDocumentIssuerView(DocumentIssuedBy documentIssuer)
        {
            var documentissuerVM = new DocumentIssueVM<DocumentIssuedBy>()
            {
                DocumentIssue = documentIssuer
            };
            return documentissuerVM;
        }

        public void Create(DocumentIssuedBy documentIssuer)
        {
            rc.DocumentsIssuedBy.Add(documentIssuer);
            rc.SaveChanges();
        }

        public void Edit(DocumentIssuedBy documentIssuer)
        {
            rc.DocumentsIssuedBy.Update(documentIssuer);
            rc.SaveChanges();
        }

        public void Delete(DocumentIssuedBy documentIssuer)
        {
            documentIssuer.Deleted = 1;
            rc.SaveChanges();
        }

    }
}
