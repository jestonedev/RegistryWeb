using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.ViewModel
{
    public class PremisesListVM : ListVM<PremisesListFilter>
    {
        public IEnumerable<Premise> Premises { get; set; }
        public IEnumerable<PremisesType> PremisesTypes { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<FundType> FundTypes { get; set; }
        //public PremisesListFilterVM PremisesListFilterVM { get; set; }
        //public PremisesListSortVM PremisesListSortVM { get; set; }

        //public PremisesListVM()
        //{
        //}

        //public PremisesListVM(RegistryContext rc, int page, int pageSize, PremisesListSortState sortOrder,
        //    string street, int? idPremisesType, int? idObjectState, int? idFundType)
        //{
        //    SetPremisesListDisplay(rc);
        //    SetPremisesListSortVM(sortOrder);
        //    SetPremisesListFilterVM(rc, street, idPremisesType, idObjectState, idFundType);
        //}


        //private void SetPremisesListDisplay(RegistryContext rc)
        //{
        //    var funds =
        //        from pCurFund in
        //            from fpa in rc.FundsPremisesAssoc
        //            join fh in rc.FundsHistory on fpa.IdFund equals fh.IdFund
        //            where fh.ExcludeRestrictionDate == null && fh.Deleted == 0 && fpa.Deleted == 0
        //            group fpa by fpa.IdPremises into gr
        //            select new
        //            {
        //                IdPremises = gr.Key,
        //                IdFund = gr.Max(s => s.IdFund)
        //            }
        //        join fh in rc.FundsHistory
        //            on pCurFund.IdFund equals fh.IdFund
        //        join ft in rc.FundTypes
        //            on fh.IdFundType equals ft.IdFundType
        //        select new
        //        {
        //            pCurFund.IdPremises,
        //            fh.IdFund,
        //            fh.IdFundType,
        //            ft.FundType
        //        };

        //    PremisesListDisplay =
        //        from p in rc.Premises
        //        join b in rc.Buildings
        //            on p.IdBuilding equals b.IdBuilding
        //        join os in rc.ObjectStates
        //            on p.IdState equals os.IdState
        //        join pt in rc.PremisesTypes
        //            on p.IdPremisesType equals pt.IdPremisesType
        //        join k in rc.KladrStreets
        //            on b.IdStreet equals k.IdStreet
        //        join f in funds
        //            on p.IdPremises equals f.IdPremises into fGroup
        //        from ff in fGroup.DefaultIfEmpty()
        //        where p.Deleted == 0
        //        select new PremisesListDisplay
        //        {
        //            Premises = p,
        //            Street = k.StreetName,
        //            House = b.House,
        //            PremisesType = pt.PremisesType,
        //            TotalArea = p.TotalArea,
        //            ObjectState = os.StateNeutral,
        //            IdFundType = ff == null ? 0 : ff.IdFundType,
        //            FundType = ff == null ? "" : ff.FundType
        //        };
        //}

        //// сортировка
        //private void SetPremisesListSortVM(PremisesListSortState sortOrder)
        //{            
        //    switch (sortOrder)
        //    {
        //        case PremisesListSortState.IdRegistryDesc:
        //            PremisesListDisplay = PremisesListDisplay.OrderByDescending(s => s.Premises.IdPremises);
        //            break;
        //        case PremisesListSortState.PremisesTypeAsc:
        //            PremisesListDisplay = PremisesListDisplay.OrderBy(s => s.PremisesType);
        //            break;
        //        case PremisesListSortState.PremisesTypeDesc:
        //            PremisesListDisplay = PremisesListDisplay.OrderByDescending(s => s.PremisesType);
        //            break;
        //        case PremisesListSortState.StateAsc:
        //            PremisesListDisplay = PremisesListDisplay.OrderBy(s => s.ObjectState);
        //            break;
        //        case PremisesListSortState.StateDesc:
        //            PremisesListDisplay = PremisesListDisplay.OrderByDescending(s => s.ObjectState);
        //            break;
        //        case PremisesListSortState.FundAsc:
        //            PremisesListDisplay = PremisesListDisplay.OrderBy(s => s.FundType);
        //            break;
        //        case PremisesListSortState.FundDesc:
        //            PremisesListDisplay = PremisesListDisplay.OrderByDescending(s => s.FundType);
        //            break;
        //        default:
        //            PremisesListDisplay = PremisesListDisplay.OrderBy(s => s.Premises.IdPremises); // RegistryNumAsc
        //            break;
        //    }

        //    PremisesListSortVM = new PremisesListSortVM(sortOrder);
        //}

        ////Фильтр
        //private void SetPremisesListFilterVM(RegistryContext rc, string street, int? idPremisesType, int? idObjectState, int? idFundType)
        //{
        //    if (idPremisesType != null && idPremisesType != 0)
        //    {
        //        PremisesListDisplay = PremisesListDisplay.Where(p => p.Premises.IdPremisesType == idPremisesType);
        //    }
        //    if (idObjectState != null && idObjectState != 0)
        //    {
        //        PremisesListDisplay = PremisesListDisplay.Where(p => p.Premises.IdState == idObjectState);
        //    }
        //    if (idFundType != null && idFundType != 0)
        //    {
        //        PremisesListDisplay = PremisesListDisplay.Where(p => p.IdFundType == idFundType);
        //    }
        //    if (!string.IsNullOrEmpty(street))
        //    {
        //        PremisesListDisplay = PremisesListDisplay.Where(p => p.Street.Contains(street));
        //    }

        //    PremisesListFilterVM = new PremisesListFilterVM(
        //            street,
        //            rc.PremisesTypes.ToList(), idPremisesType,
        //            rc.ObjectStates.ToList(), idObjectState,
        //            rc.FundTypes.ToList(), idFundType);
        //}
    }
}
