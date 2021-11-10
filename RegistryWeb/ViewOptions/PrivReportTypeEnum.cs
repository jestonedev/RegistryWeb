using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions
{
    public enum PrivReportTypeEnum
    {
        StatByFirstPrivatization = 1,
        NoEgrpReg = 2,
        ForProcuracy = 3,
        PrivEstateWithAreaDetails = 4,
        PrivRoomsInPremiseWithSettle  = 5,
        PrivHouses = 6,
        PrivPremises = 7,
        PrivSubPremises = 8,
        Unprivatization = 9,
        ByAddress = 10,
        ByRegion = 11,
        ByApplicationDate = 12,
        OrderAdditional = 13,
        OrderExcludePremises = 14,
        RegistryForCpmu = 15,
        StatIssuedNotRegistred = 16,
        StatByIssueDate = 17,
        StatByIssueCivilDate = 18,
        StatByRegDate = 19,
        StatByRegEgrpDate = 20,
        StatByWarrantPerson = 21,
        StatByRefusenik = 22,
        StatByRefuse = 23,
        StatByContractors = 24
    }
}
