using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Models;

namespace RegistryServices.Models.KumiPayments
{
    [Serializable]
    public class KumiPaymentsUploadStateModel
    {
        public List<Tuple<KumiPayment, string>> PaymentsDicitionaryBindErrors { get; set; }
        public List<Tuple<KumiMemorialOrder, string>> MemorialOrdersDicitionaryBindErrors { get; set; }
        public List<Tuple<KumiPayment, string>> CheckExtractErrors { get; set; }
        public List<Tuple<KumiMemorialOrder, string>> BindMemorialOrdersErrors { get; set; }
        public List<Tuple<KumiMemorialOrder, KumiPayment>> BindedMemorialOrders { get; set; }
        public List<KumiMemorialOrder> InsertedMemorialOrders { get; set; }
        public List<KumiMemorialOrder> SkipedMemorialOrders { get; set; }
        public List<KumiPayment> PaymentsWithoutExtract { get; set; }
        public List<KumiPayment> KnownPayments { get; set; }
        public List<KumiPayment> UnknownPayments { get; set; }
        public List<KumiPayment> SkipedPayments { get; set; }
        public List<KumiPayment> InsertedPayments { get; set; }
        public List<KumiPayment> UpdatedPayments { get; set; }
        public List<KumiPayment> BindedExtractsToDbPayments { get; set; }
        public List<KumiPaymentExtract> UnknownPaymentExtracts { get; set; }

        public KumiPaymentsUploadStateModel()
        {
            PaymentsDicitionaryBindErrors = new List<Tuple<KumiPayment, string>>();
            CheckExtractErrors = new List<Tuple<KumiPayment, string>>();
            BindMemorialOrdersErrors = new List<Tuple<KumiMemorialOrder, string>>();
            BindedMemorialOrders = new List<Tuple<KumiMemorialOrder, KumiPayment>>();
            InsertedMemorialOrders = new List<KumiMemorialOrder>();
            SkipedMemorialOrders = new List<KumiMemorialOrder>();
            PaymentsWithoutExtract = new List<KumiPayment>();
            KnownPayments = new List<KumiPayment>();
            UnknownPayments = new List<KumiPayment>();
            SkipedPayments = new List<KumiPayment>();
            InsertedPayments = new List<KumiPayment>();
            UpdatedPayments = new List<KumiPayment>();
            BindedExtractsToDbPayments = new List<KumiPayment>();
            UnknownPaymentExtracts = new List<KumiPaymentExtract>();
            MemorialOrdersDicitionaryBindErrors = new List<Tuple<KumiMemorialOrder, string>>();
        }
    }
}
