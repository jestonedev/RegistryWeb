using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryPaymentsLoader.TffFileLoaders;
using RegistryDb.Models.Entities.Common;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;
using RegistryServices.Classes;
using RegistryServices.Models.KumiPayments;

namespace RegistryServices.DataServices.KumiPayments
{
    public class KumiPaymentsMemorialOrdersService
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityService;

        public KumiPaymentsMemorialOrdersService(
            RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public KumiPaymentUf GetKumiPaymentUf(int idPaymentUf)
        {
            return registryContext.KumiPaymentUfs
                .Include(r => r.KbkType)
                .Include(r => r.Payment).ThenInclude(r => r.PaymentDocCode)
                .Include(r => r.Payment).ThenInclude(r => r.KbkType)
                .AsNoTracking()
                .FirstOrDefault(r => r.IdPaymentUf == idPaymentUf);
        }

        public List<KumiPaymentUf> GetKumiPaymentUfs(DateTime dateUf)
        {
            return registryContext.KumiPaymentUfs
                    .Include(r => r.KbkType)
                    .Include(r => r.Payment).ThenInclude(r => r.PaymentDocCode)
                    .Include(r => r.Payment).ThenInclude(r => r.KbkType)
                    .AsNoTracking()
                    .Where(r => r.DateUf == dateUf).ToList();
        }

        public int? GetKumiPaymentUfsLastNumber()
        {
            var lastNum = registryContext.KumiPaymentUfs.LastOrDefault()?.NumUf;
            if (int.TryParse(lastNum, out int lastNumInt)) return lastNumInt;
            return null;
        }

        public byte[] GetPaymentUfsFile(List<KumiPaymentUf> paymentUfs, KumiPaymentSettingSet paymentSettings, SelectableSigner signer, DateTime signDate)
        {
            return new TXUF180101FileCreator().CreateFile(paymentUfs, paymentSettings, securityService.Executor, signer, signDate);
        }

        public Dictionary<Tuple<int, int>, KumiPayment> GetPaymentsByOrders(List<KumiMemorialOrder> orders)
        {
            var idOrders = orders.Select(r => r.IdOrder).ToList();
            var orderNums = orders.Select(r => r.NumDocument).ToList();
            var paymentRows = from assocRow in registryContext.KumiMemorialOrderPaymentAssocs
                              join paymentRow in registryContext.KumiPayments
                              on assocRow.IdPayment equals paymentRow.IdPayment
                              where idOrders.Contains(assocRow.IdOrder)
                              select new
                              {
                                  assocRow.IdOrder,
                                  paymentRow
                              };

            var paymentRowsDict = new Dictionary<Tuple<int, int>, KumiPayment>();
            foreach (var paymentRow in paymentRows)
            {
                paymentRowsDict.Add(new Tuple<int, int>(paymentRow.IdOrder, paymentRow.paymentRow.IdPayment), paymentRow.paymentRow);
            }

            return paymentRowsDict;
        }

        public List<KumiMemorialOrder> GetKumiPaymentMemorialOrderPairs(int idOrder)
        {
            var order = registryContext.KumiMemorialOrders.FirstOrDefault(r => r.IdOrder == idOrder);
            if (order == null) throw new Exception("Не удалось найти мемориальный ордер");
            return registryContext.KumiMemorialOrders.Where(r => r.NumDocument == order.NumDocument && r.DateDocument == order.DateDocument
                        && r.IdGroup == order.IdGroup).ToList();
        }

        public IQueryable<KumiMemorialOrder> GetMemorialOrders(MemorialOrderFilter filterOptions)
        {
            var query = registryContext.KumiMemorialOrders.Include(mo => mo.MemorialOrderPaymentAssocs).Where(mo => mo.MemorialOrderPaymentAssocs.Count == 0);
            if (!string.IsNullOrEmpty(filterOptions.NumDocument))
                query = query.Where(r => r.NumDocument == filterOptions.NumDocument);
            if (filterOptions.DateDocument != null)
                query = query.Where(r => r.DateDocument == filterOptions.DateDocument);
            if (filterOptions.Sum != null)
                query = query.Where(r => r.SumZach == filterOptions.Sum);
            if (!string.IsNullOrEmpty(filterOptions.Kbk))
                query = query.Where(r => r.Kbk != null && r.Kbk.Contains(filterOptions.Kbk));
            if (!string.IsNullOrEmpty(filterOptions.Okato))
                query = query.Where(r => r.Okato != null && r.Okato.Contains(filterOptions.Okato));
            return query;
        }

        private void AddPaymentCorrections(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            var date = memorialOrder.DateDocument;
            if (payment.Sum != memorialOrder.SumZach)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    IdCorrection = 0,
                    Payment = payment,
                    FieldName = "Sum",
                    FieldValue = payment.Sum.ToString(),
                    Date = date
                });
            }
            if (payment.IdKbkType != memorialOrder.IdKbkType)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "IdKbkType",
                    FieldValue = payment.IdKbkType?.ToString(),
                    Date = date
                });
            }
            if (payment.Kbk != memorialOrder.Kbk)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "Kbk",
                    FieldValue = payment.Kbk,
                    Date = date
                });
            }
            if (payment.TargetCode != memorialOrder.TargetCode)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "TargetCode",
                    FieldValue = payment.TargetCode,
                    Date = date
                });
            }
            if (payment.Okato != memorialOrder.Okato)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "Okato",
                    FieldValue = payment.Okato,
                    Date = date
                });
            }
            if (payment.RecipientInn != memorialOrder.InnAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "RecipientInn",
                    FieldValue = payment.RecipientInn,
                    Date = date
                });
            }
            if (payment.RecipientKpp != memorialOrder.KppAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "RecipientKpp",
                    FieldValue = payment.RecipientKpp,
                    Date = date
                });
            }
        }

        private KumiPayment ApplyMemorialOrder(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            AddPaymentCorrections(payment, memorialOrder);
            if (memorialOrder.SumZach < 0)
            {
                if (payment.Kbk != memorialOrder.Kbk || payment.IdKbkType != memorialOrder.IdKbkType || payment.TargetCode != memorialOrder.TargetCode ||
                    payment.Okato != memorialOrder.Okato || payment.RecipientInn != memorialOrder.InnAdb || payment.RecipientKpp != memorialOrder.KppAdb)
                    throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие данных мемориального ордера на списание по платежу {0}", payment.Guid));
                payment.Sum = payment.Sum + memorialOrder.SumZach;
            }
            else
                payment.Sum = memorialOrder.SumZach;
            if (payment.Sum < 0)
            {
                throw new KumiPaymentCheckVtOperException(string.Format("При применении мемориального ордера к платежу {0} скорректированная сумма получилась отрицательной", payment.Guid));
            }
            payment.IdKbkType = memorialOrder.IdKbkType;
            payment.Kbk = memorialOrder.Kbk;
            payment.TargetCode = memorialOrder.TargetCode;
            payment.Okato = memorialOrder.Okato;
            payment.RecipientInn = memorialOrder.InnAdb;
            payment.RecipientKpp = memorialOrder.KppAdb;

            // Связываем мемориальные ордера с платежом
            if (payment.MemorialOrderPaymentAssocs == null)
            {
                payment.MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc>();
            }
            payment.MemorialOrderPaymentAssocs.Add(new KumiMemorialOrderPaymentAssoc { Order = memorialOrder });

            return payment;
        }

        public void ApplyMemorialOrderToPayment(KumiPayment payment, int idOrder, out bool updatedExistsPayment)
        {
            updatedExistsPayment = true;
            // Если ордер уже подвязан, то пропускаем
            var mo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(r => r.IdOrder == idOrder);

            if (mo == null) throw new ApplicationException("Не удалось найти мемориальный ордер");

            if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any())
                throw new ApplicationException("Мемориальный оредр уже привязан к платежу");

            if (payment == null) throw new ApplicationException("Не удалось найти платеж");

            if ((payment.PaymentCharges != null && payment.PaymentCharges.Any()) || (payment.PaymentClaims != null && payment.PaymentClaims.Any()))
                throw new ApplicationException("Платеж распределен. Для привязки мемориального ордера необходимо отменить распределение платежа");

            if (payment.IsConsolidated != 0)
                throw new ApplicationException("Платеж является сводным платежным поручением. Нельзя уточнять сводные платежные поручения после подругзки детализирующего реестра платежей");

            var idParentPayment = payment.IdPayment;

            var copyPayment = false;

            ClearMemorialOrderFieldsValues(mo);

            if (mo.SumZach >= 0 &&
                 (payment.Kbk != mo.Kbk || payment.IdKbkType != mo.IdKbkType || payment.TargetCode != mo.TargetCode ||
                     payment.Okato != mo.Okato || payment.RecipientInn != mo.InnAdb || payment.RecipientKpp != mo.KppAdb))
            {
                copyPayment = true;
            }


            if (copyPayment)
                payment = payment.Copy(true);

            payment = ApplyMemorialOrder(payment, mo);

            if (payment.IdPayment != 0)
            {
                var corrections = payment.PaymentCorrections;
                payment.PaymentCorrections = null;
                registryContext.KumiPayments.Update(payment);
                foreach (var correction in corrections.Where(r => r.IdCorrection == 0))
                {
                    correction.IdPayment = payment.IdPayment;
                    registryContext.KumiPaymentCorrections.Add(correction);
                }
                updatedExistsPayment = true;
            }
            else
            {
                payment.IdParentPayment = idParentPayment;
                registryContext.KumiPayments.Add(payment);
                updatedExistsPayment = false;
            }
            registryContext.SaveChanges();
        }

        public void UploadMemorialOrders(List<KumiMemorialOrder> memorialOrders, KumiPaymentGroup group, KumiPaymentsUploadStateModel loadState)
        {
            foreach (var mo in memorialOrders.OrderBy(r => r.Guid).ThenBy(r => r.NumDocument).ThenBy(r => r.DateDocument).ThenBy(r => r.SumZach))
            {
                try
                {
                    var localMo = BindDictionariesToMemorialOrder(mo);
                    localMo.PaymentGroup = group;
                    ClearMemorialOrderFieldsValues(mo);

                    var dbMo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs).FirstOrDefault(
                        r => r.Guid == mo.Guid &&
                                r.NumDocument == localMo.NumDocument &&
                                r.DateDocument == localMo.DateDocument &&
                                r.SumIn == mo.SumIn &&
                                r.SumZach == mo.SumZach &&
                                r.IdKbkType == localMo.IdKbkType &&
                                r.Kbk == mo.Kbk &&
                                r.TargetCode == mo.TargetCode &&
                                r.Okato == mo.Okato &&
                                r.KppAdb == mo.KppAdb &&
                                r.InnAdb == mo.InnAdb);
                    if (dbMo == null)
                    {
                        registryContext.KumiMemorialOrders.Add(localMo);
                        loadState.InsertedMemorialOrders.Add(localMo);
                    }
                    else
                    {
                        localMo.IdOrder = dbMo.IdOrder;
                        localMo.MemorialOrderPaymentAssocs = dbMo.MemorialOrderPaymentAssocs;
                        loadState.SkipedMemorialOrders.Add(localMo);
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.MemorialOrdersDicitionaryBindErrors.Add(new RegistryTuple<KumiMemorialOrder, string>(mo, e.Message));
                    continue;
                }

                // Если ордер уже подвязан, то пропускаем
                if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any()) continue;

                // Если сумма ордера отрицательная, то производим списание с платежа, соответствующего всем критериям
                // Если сумма положительная, то создаем новый платеж на основании имеющегося и обновляем целевые строки
                var dbPayments = registryContext.KumiPayments
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .AsNoTracking()
                    .Where(r => r.Guid == mo.Guid &&
                        r.PaymentUfs.Count(ru => ru.NumUf == mo.NumDocument && ru.DateUf == mo.DateDocument) > 0);

                var dbConcretPayments = dbPayments.Where(r => r.Kbk == mo.Kbk && r.IdKbkType == mo.IdKbkType && r.TargetCode == mo.TargetCode &&
                       r.Okato == mo.Okato && r.RecipientInn == mo.InnAdb && r.RecipientKpp == mo.KppAdb);

                var copyPayment = true;

                if (dbConcretPayments.Count() > 0)
                {
                    dbPayments = dbConcretPayments;
                    copyPayment = false;
                }

                // Если платежей больше не строго один, то пропускаем
                if (dbPayments.Count() != 1) continue;

                var dbPayment = dbPayments.First();

                // Если платеж уже распределен, то пропускаем
                if ((dbPayment.PaymentCharges != null && dbPayment.PaymentCharges.Any()) || (dbPayment.PaymentClaims != null && dbPayment.PaymentClaims.Any()))
                    continue;

                var idParentPayment = dbPayment.IdPayment;

                if (copyPayment)
                    dbPayment = dbPayment.Copy(true);

                try
                {
                    dbPayment = ApplyMemorialOrder(dbPayment, mo);

                    loadState.BindedMemorialOrders.Add(new RegistryTuple<KumiMemorialOrder, KumiPayment>(mo, dbPayment));

                    if (dbPayment.IdPayment != 0)
                    {
                        var corrections = dbPayment.PaymentCorrections;
                        dbPayment.PaymentCorrections = null;
                        registryContext.KumiPayments.Update(dbPayment);
                        foreach (var correction in corrections.Where(r => r.IdCorrection == 0))
                        {
                            correction.IdPayment = dbPayment.IdPayment;
                            registryContext.KumiPaymentCorrections.Add(correction);
                        }
                    }
                    else
                    {
                        dbPayment.IdParentPayment = idParentPayment;
                        registryContext.KumiPayments.Add(dbPayment);
                    }
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.BindMemorialOrdersErrors.Add(new RegistryTuple<KumiMemorialOrder, string>(mo, e.Message));
                }
            }
        }

        private List<KumiKbkType> kumiKbkTypes { get; set; }

        private KumiMemorialOrder BindDictionariesToMemorialOrder(KumiMemorialOrder order)
        {
            if (kumiKbkTypes == null)
                kumiKbkTypes = registryContext.KumiKbkTypes.ToList();
            if (order.KbkType != null)
            {
                var kbkType = kumiKbkTypes.FirstOrDefault(r => r.Code == order.KbkType.Code);
                if (kbkType == null && order.KbkType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный тип КБК {0} в мемориальном ордере {1}", order.KbkType.Code, order.Guid));
                order.KbkType = null;
                order.IdKbkType = kbkType?.IdKbkType;
            }

            return order;
        }

        private void ClearMemorialOrderFieldsValues(KumiMemorialOrder mo)
        {
            if (string.IsNullOrEmpty(mo.InnAdb))
                mo.InnAdb = null;
            if (string.IsNullOrEmpty(mo.KppAdb))
                mo.KppAdb = null;
            if (string.IsNullOrEmpty(mo.TargetCode))
                mo.TargetCode = null;
            if (string.IsNullOrEmpty(mo.Kbk))
                mo.Kbk = null;
            if (string.IsNullOrEmpty(mo.NumDocument))
                mo.NumDocument = null;
        }

        public KumiPayment CreatePaymentByMemorialOrder(int idOrder)
        {
            var mo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(r => r.IdOrder == idOrder);

            if (mo == null) throw new ApplicationException("Не удалось найти мемориальный ордер");

            if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any())
                throw new ApplicationException("Мемориальный оредр уже привязан к платежу");

            if (mo.SumZach < 0)
                throw new ApplicationException("Нельзя создать платеж на основе ордера с отрицательной суммой");

            var payment = new KumiPayment
            {
                IdSource = 1,
                Sum = mo.SumZach,
                Kbk = mo.Kbk,
                IdKbkType = mo.IdKbkType,
                Guid = mo.Guid,
                Okato = mo.Okato,
                RecipientInn = mo.InnAdb,
                RecipientKpp = mo.KppAdb,
                TargetCode = mo.TargetCode,
                MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc> {
                    new KumiMemorialOrderPaymentAssoc
                    {
                        IdOrder = idOrder
                    }
                }
            };
            registryContext.KumiPayments.Add(payment);
            registryContext.SaveChanges();
            return payment;

        }

    }
}
