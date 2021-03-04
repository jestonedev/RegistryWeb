﻿namespace RegistryWeb.Models.Entities
{
    public class PrivContract
    {
        public int IdContract { get; set; }
        public string RegNumber { get; set; }
        public bool IsRefusenik { get; set; }
        public bool IsRefuse { get; set; }
        public bool IsRasprivatization { get; set; }
        public bool IsRelocation { get; set; }
        public int? IdExecutor { get; set; }

        public virtual Executor ExecutorNavigation { get; set; }
    }
}
