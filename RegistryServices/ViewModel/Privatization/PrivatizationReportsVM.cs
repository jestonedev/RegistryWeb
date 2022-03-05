using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.SqlViews;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Privatization
{
    public class PrivatizationReportsVM
    {
        public List<KladrRegion> Regions { get; set; }
        public List<KladrStreet> Streets { get; set; }
        public List<KeyValuePair<int, string>> PremiseTypes { get; set; } // + квартира, комната, квартира с подселением, дом - без привязки к справочнику БД
        public List<KeyValuePair<int, string>> LiteraTypes { get; set; } // с литерой "п", без литеры "п" - без привязки к справочнику
        public List<Executor> Executors { get; set; }
        public List<KeyValuePair<int, string>> Order { get; set; } // сортировка по дате, по рег. номеру - без привязки к справочнику
    }
}
