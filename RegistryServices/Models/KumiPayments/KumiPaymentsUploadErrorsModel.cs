using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Models;
using RegistryServices.Classes;

namespace RegistryServices.Models.KumiPayments
{
    [Serializable]
    public class KumiPaymentsUploadStateModel
    {
        public List<RegistryTuple<KumiPayment, string>> PaymentsDicitionaryBindErrors { get; set; }
        public List<RegistryTuple<KumiMemorialOrder, string>> MemorialOrdersDicitionaryBindErrors { get; set; }
        public List<RegistryTuple<KumiPayment, string>> CheckExtractErrors { get; set; }
        public List<RegistryTuple<KumiMemorialOrder, string>> BindMemorialOrdersErrors { get; set; }
        public List<RegistryTuple<KumiMemorialOrder, KumiPayment>> BindedMemorialOrders { get; set; }
        public List<KumiMemorialOrder> InsertedMemorialOrders { get; set; }
        public List<KumiMemorialOrder> SkipedMemorialOrders { get; set; }
        public List<KumiPayment> PaymentsWithoutExtract { get; set; }
        public List<KumiPayment> KnownPayments { get; set; }
        public List<KumiPayment> UnknownPayments { get; set; }
        public List<KumiPayment> ReturnPayments { get; set; }
        public List<KumiPayment> SkipedPayments { get; set; }
        public List<KumiPayment> InsertedPayments { get; set; }
        public List<KumiPayment> UpdatedPayments { get; set; }
        public List<KumiPayment> BindedExtractsToDbPayments { get; set; }
        public List<KumiPaymentExtract> UnknownPaymentExtracts { get; set; }
        public List<RegistryTuple<KumiPayment, KumiAccount>> AutoDistributedPayments { get; set; }

        public KumiPaymentsUploadStateModel()
        {
            PaymentsDicitionaryBindErrors = new List<RegistryTuple<KumiPayment, string>>();
            CheckExtractErrors = new List<RegistryTuple<KumiPayment, string>>();
            BindMemorialOrdersErrors = new List<RegistryTuple<KumiMemorialOrder, string>>();
            BindedMemorialOrders = new List<RegistryTuple<KumiMemorialOrder, KumiPayment>>();
            InsertedMemorialOrders = new List<KumiMemorialOrder>();
            SkipedMemorialOrders = new List<KumiMemorialOrder>();
            PaymentsWithoutExtract = new List<KumiPayment>();
            KnownPayments = new List<KumiPayment>();
            UnknownPayments = new List<KumiPayment>();
            ReturnPayments = new List<KumiPayment>();
            SkipedPayments = new List<KumiPayment>();
            InsertedPayments = new List<KumiPayment>();
            UpdatedPayments = new List<KumiPayment>();
            BindedExtractsToDbPayments = new List<KumiPayment>();
            UnknownPaymentExtracts = new List<KumiPaymentExtract>();
            MemorialOrdersDicitionaryBindErrors = new List<RegistryTuple<KumiMemorialOrder, string>>();
            AutoDistributedPayments = new List<RegistryTuple<KumiPayment, KumiAccount>>();
        }
    }
}
