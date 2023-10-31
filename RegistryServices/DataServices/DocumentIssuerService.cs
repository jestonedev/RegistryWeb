using RegistryDb.Models;
using RegistryWeb.ViewOptions;
using System.Linq;
using System;
using RegistryServices.ViewModel.Tenancies;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.DataFilterServices;

namespace RegistryWeb.DataServices
{
    public class DocumentIssuerService
    {
        private readonly RegistryContext rc;
        private readonly IFilterService<DocumentIssuedBy, FilterOptions> filterService;

        public DocumentIssuerService(RegistryContext rc,
            FilterServiceFactory<IFilterService<DocumentIssuedBy, FilterOptions>> filterServiceFactory)
        {
            this.rc = rc;
            filterService = filterServiceFactory.CreateInstance();
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
            viewModel.DocumentIssues = filterService.GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
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
