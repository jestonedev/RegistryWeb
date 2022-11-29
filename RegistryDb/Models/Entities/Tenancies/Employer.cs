using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Tenancies
{
    public partial class Employer
    {
        public Employer()
        {
            TenancyProcesses = new List<TenancyProcess>();
        }

        public int IdEmployer { get; set; }
        public string EmployerName { get; set; }
        public IList<TenancyProcess> TenancyProcesses { get; set; }
    }
}
