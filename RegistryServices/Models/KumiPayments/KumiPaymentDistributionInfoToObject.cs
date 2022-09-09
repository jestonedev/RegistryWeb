﻿using RegistryServices.Enums;
using System;

namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfoToObject: KumiPaymentDistributionInfo
    {
        public KumiPaymentDistributeToEnum ObjectType { get; set; }
        public int IdCharge { get; set; }
    }
}
