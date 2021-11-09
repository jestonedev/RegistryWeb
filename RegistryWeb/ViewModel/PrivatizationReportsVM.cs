using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models.SqlViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
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
