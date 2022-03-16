using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.TffFileCreators;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    public class TXUF180101FileCreator : TffFileCreator
    {
        public override string Version => "TXUF180101";

        public byte[] CreateFile(List<KumiPaymentUf> paymentUfs, KumiPaymentSettingSet paymentSettings, Executor currentExecutor, SelectableSigner signer, DateTime signDate)
        {
            var fk = GetFk();
            var from = GetFrom(paymentSettings);
            var to = GetTo(paymentSettings);
            var result = fk + Environment.NewLine + from + Environment.NewLine + to + Environment.NewLine;
            foreach(var paymentUf in paymentUfs)
            {
                var uf = GetUf(paymentUf, paymentSettings, currentExecutor, signer, signDate);
                var ufpp = GetUfpp(paymentUf);
                var ufppn = GetUfppn(paymentUf);
                result += uf + Environment.NewLine + ufpp + Environment.NewLine + ufppn + Environment.NewLine;
            }
            return Encoding.UTF8.GetBytes(result);
        }

        private string GetFk()
        {
            return string.Format("FK|{0}||||", Version); ;
        }

        private string GetFrom(KumiPaymentSettingSet paymentSettings)
        {
            return string.Format("FROM|{0}|{1}|{2}|", 
                paymentSettings.BudgetLevel, paymentSettings.CodeUbp, paymentSettings.NameUbp);
        }

        private string GetTo(KumiPaymentSettingSet paymentSettings)
        {
            return string.Format("TO|{0}|{1}|",
                paymentSettings.CodeTofk, paymentSettings.NameTofk);
        }

        private string GetUf(KumiPaymentUf paymentUf, KumiPaymentSettingSet paymentSettings, Executor currentExecutor, SelectableSigner signer, DateTime signDate)
        {
            var signerSnp = signer.Name.Substring(0, 1) + "." + (signer.Patronymic == null ? " " : signer.Patronymic.Substring(0, 1) + ". ") + signer.Surname;
            if (signerSnp.Length > 50)
                signerSnp = signerSnp?.Substring(0, 50);


            var signerPost = signer.Post;
            if (signerPost.Length > 50)
            {
                signerPost = signerPost?.Substring(0, 50);
            }

            var executorNameParts = currentExecutor.ExecutorName.Split(' ', 3);
            var executorSnp = "";
            if (executorNameParts.Length < 2)
            {
                executorSnp = currentExecutor.ExecutorName;
            } else
            if (executorNameParts.Length == 2)
            {
                executorSnp = executorNameParts[0] + " " + executorNameParts[1].Substring(0, 1) + ".";
            } else
            {
                executorSnp = executorNameParts[0] + " " + executorNameParts[1].Substring(0, 1) + "." + executorNameParts[2].Substring(0, 1) + "." ;
            }

            if (executorSnp.Length > 50)
            {
                executorSnp = executorSnp?.Substring(0, 50);
            }

            return string.Format("UF||{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}||{13}|{14}|{15}|{16}|{17}||{18}|{19}|{20}|{21}|{22}|{23}|{24}|||||||",
                paymentUf.NumUf, paymentUf.DateUf.ToString("dd.MM.yyyy"), paymentSettings.NameUbp, paymentSettings.CodeUbp, paymentSettings.AccountUbp,
                paymentSettings.NameGrs, paymentSettings.GlavaGrs, paymentSettings.NameBudget, paymentSettings.NameFo, paymentSettings.OkpoFo, paymentSettings.AccountFo,
                paymentSettings.NameTofk, paymentSettings.CodeTofk, paymentUf.Payment.NumDocumentIndicator?.Replace("|", " "), paymentUf.Payment.DateDocumentIndicator?.ToString("dd.MM.yyyy"),
                paymentUf.Payment.PayerName?.Replace("|"," "), paymentUf.Payment.PayerInn, paymentUf.Payment.PayerKpp, paymentUf.Payment.PayerAccount, signerPost, 
                signerSnp, currentExecutor.ExecutorPost, executorSnp, currentExecutor.Phone, signDate.ToString("dd.MM.yyyy"));
        }

        private string GetUfpp(KumiPaymentUf paymentUf)
        {
            // TODO Платежное поручение?
            return string.Format("UFPP|1|{0}|{1}|Платежное поручение|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}||||",
               paymentUf.Payment.Guid, paymentUf.Payment.PaymentDocCode.Code, paymentUf.Payment.NumDocument, paymentUf.Payment.DateDocument?.ToString("dd.MM.yyyy"),
               paymentUf.Payment.RecipientName?.Replace("|", " "), paymentUf.Payment.RecipientInn, paymentUf.Payment.RecipientKpp, paymentUf.Payment.Okato,
               paymentUf.Payment.Kbk, paymentUf.Payment.KbkType.Code, paymentUf.Payment.TargetCode, paymentUf.Payment.Sum.ToString().Replace(",", "."), 
               paymentUf.Payment.Purpose?.Replace("|", " "));
        }

        private string GetUfppn(KumiPaymentUf paymentUf)
        {
            return string.Format("UFPP_N|1|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}||",
               paymentUf.RecipientName?.Replace("|", " "), paymentUf.RecipientInn, paymentUf.RecipientKpp, paymentUf.Okato, paymentUf.RecipientAccount,
               paymentUf.Kbk, paymentUf.KbkType.Code, paymentUf.TargetCode, paymentUf.Sum?.ToString().Replace(",", "."), paymentUf.Purpose?.Replace("|", " "));
        }
    }
}
