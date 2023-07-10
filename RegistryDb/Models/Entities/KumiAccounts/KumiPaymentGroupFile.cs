﻿using System;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentGroupFile
    {
        public int IdFile { get; set; }
        public int IdGroup { get; set; }
        public string FileName { get; set; }
        public string FileVersion { get; set; }
        public DateTime? NoticeDate { get; set; }
        public virtual KumiPaymentGroup PaymentGroup { get; set; }
    }
}