using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class DocumentIssueVM<T>
    {
        public List<DocumentIssuedBy> DocumentIssues { get; set; }
        public DocumentIssuedBy DocumentIssue { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
