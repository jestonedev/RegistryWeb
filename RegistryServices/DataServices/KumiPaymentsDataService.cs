using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryServices.ViewModel.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.TffStrings;
using RegistryServices.Models;
using RegistryPaymentsLoader.Models;
using RegistryServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.DataServices
{

    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        private SecurityService securityService;

        public KumiPaymentsDataService(RegistryContext registryContext, AddressesDataService addressesDataService, SecurityService securityService) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
        }

        public KumiPaymentsUploadStateModel UploadInfoFromTff(List<TffString> tffStrings, List<KumiPaymentGroupFile> kumiPaymentGroupFiles)
        {
            var loadState = new KumiPaymentsUploadStateModel();

            var extracts = tffStrings.Where(r => r is TffStringVT)
                .Select(r => ((TffStringVT)r).ToExtract())
                .Where(r => !r.IsMemorialOrder()).ToList();

            var memorialOrders = tffStrings.Where(r => r is TffStringVT)
                .Select(r => ((TffStringVT)r).ToExtract())
                .Where(r => r.IsMemorialOrder())
                .Select(r => r.ToMemorialOrder()).ToList();

            var knownPayments = 
                tffStrings.Where(r => r is TffStringBD).Select(r => ((TffStringBD)r).ToPayment()).ToList();

            var unknownPayments = tffStrings.Where(r => r is TffStringZF).Select(r => ((TffStringZF)r).ToPayment()).ToList();

            var payments = knownPayments.Union(unknownPayments);

            var group = new KumiPaymentGroup {
                Date = DateTime.Now,
                PaymentGroupFiles = kumiPaymentGroupFiles,
                User = securityService.User.UserName
            };
            registryContext.KumiPaymentGroups.Add(group);

            UploadPayments(payments, group, extracts, loadState);
            UploadMemorialOrders(memorialOrders, group, loadState);

            registryContext.SaveChanges();

            return loadState;
        }
        
        private void UploadMemorialOrders(List<KumiMemorialOrder> memorialOrders, KumiPaymentGroup group, KumiPaymentsUploadStateModel loadState)
        {
            foreach (var mo in memorialOrders.OrderBy(r => r.Guid).ThenBy(r => r.NumDocument).ThenBy(r => r.DateDocument).ThenBy(r => r.SumZach))
            {
                try
                {
                    var localMo = BindDictionariesToMemorialOrder(mo);
                    localMo.PaymentGroup = group;

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
                    loadState.MemorialOrdersDicitionaryBindErrors.Add(new Tuple<KumiMemorialOrder, string>(mo, e.Message));
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
                       r.Okato == mo.Okato && r.PayerInn == mo.InnAdb && r.PayerKpp == mo.KppAdb);

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

                if (copyPayment)
                    dbPayment = dbPayment.Copy(true);

                try
                {
                    dbPayment = ApplyMemorialOrder(dbPayment, mo);

                    loadState.BindedMemorialOrders.Add(new Tuple<KumiMemorialOrder, KumiPayment>(mo, dbPayment));

                    if (dbPayment.IdPayment != 0)
                    {
                        var corrections = dbPayment.PaymentCorrections;
                        dbPayment.PaymentCorrections = null;
                        registryContext.KumiPayments.Update(dbPayment);
                        foreach (var correction in corrections.Where(r => r.IdCorrection == 0))
                        {
                            correction.IdPayment = dbPayment.IdPayment;
                            registryContext.PaymentCorrections.Add(correction);
                        }
                    }
                    else
                    {
                        registryContext.KumiPayments.Add(dbPayment);
                    }
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.BindMemorialOrdersErrors.Add(new Tuple<KumiPayment, string>(dbPayment, e.Message));
                }
            }
        }

        private void UploadPayments(IEnumerable<KumiPayment> payments, KumiPaymentGroup group, List<KumiPaymentExtract> extracts, KumiPaymentsUploadStateModel loadState)
        {
            var resultPayments = new List<KumiPayment>();
            var appliedExtracts = new List<KumiPaymentExtract>();

            foreach (var payment in payments)
            {
                var paymentLocal = payment;

                // Подготовка платежа к загрузке
                paymentLocal.PaymentGroup = group;
                try
                {
                    paymentLocal = BindDictionariesToPayment(payment);
                    var paymentExtracts = extracts.Where(r => r.Guid == paymentLocal.Guid).ToList();
                    if (paymentExtracts.Any())
                    {
                        if (paymentExtracts.Count > 1)
                        {
                            throw new KumiPaymentCheckVtOperException(string.Format("Количество строк выписки по платежу {0} больше одной", payment.Guid));
                        }
                        var extract = paymentExtracts.First();
                        paymentLocal = ApplyPaymentExtract(paymentLocal, extract);
                        appliedExtracts.Add(extract);
                    }
                    else
                    {
                        resultPayments.Add(paymentLocal);
                        loadState.PaymentsWithoutExtract.Add(paymentLocal);
                    }

                    switch (paymentLocal.IdSource)
                    {
                        case 4:
                        case 5:
                            loadState.UnknownPayments.Add(paymentLocal);
                            break;
                        case 2:
                        case 3:
                            loadState.KnownPayments.Add(paymentLocal);
                            break;
                        default:
                            throw new KumiPaymentBindDictionaryException(string.Format("Неподдерживаемый источник платежа {0} - {1}", payment.Guid, payment.IdSource));
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.PaymentsDicitionaryBindErrors.Add(new Tuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.CheckExtractErrors.Add(new Tuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }

                // Загрузка в БД
                var dbPayments = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .AsNoTracking()
                    .Where(v => v.Guid == payment.Guid);

                if (dbPayments.Count() > 1)
                {
                    loadState.SkipedPayments.Add(payment);
                } else
                if (dbPayments.Count() == 1)
                {
                    var dbPayment = dbPayments.First();
                    if (dbPayment.PaymentClaims.Any() || dbPayment.PaymentCharges.Any() || dbPayment.PaymentCorrections.Any() ||
                            dbPayment.MemorialOrderPaymentAssocs.Any() || dbPayment.PaymentUfs.Any())
                    {
                        loadState.SkipedPayments.Add(payment);
                    }
                    else
                    {
                        payment.IdPayment = dbPayment.IdPayment;
                        registryContext.KumiPayments.Update(payment);
                        loadState.UpdatedPayments.Add(payment);
                    }
                } else
                {
                    registryContext.KumiPayments.Add(payment);
                    loadState.InsertedPayments.Add(payment);
                }
            }

            var notAppliedExtracts = extracts.Except(appliedExtracts);
            foreach(var extract in notAppliedExtracts)
            {
                // Загрузка в БД
                var dbPayments = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .Where(v => v.Guid == extract.Guid);
                if (dbPayments.Count() == 1)
                {
                    var dbPayment = dbPayments.First();
                    if (!dbPayment.PaymentClaims.Any() && !dbPayment.PaymentCharges.Any() && !dbPayment.PaymentCorrections.Any() &&
                            !dbPayment.MemorialOrderPaymentAssocs.Any() && !dbPayment.PaymentUfs.Any())
                    {
                        dbPayment = ApplyPaymentExtract(dbPayment, extract);
                        registryContext.KumiPayments.Update(dbPayment);
                        loadState.BindedExtractsToDbPayments.Add(dbPayment);
                    }
                } else
                {
                    loadState.UnknownPaymentExtracts.Add(extract);
                }
            }
        }

        // Buffer
        private List<KumiPaymentDocCode> kumiPaymentDocCodes { get; set; }

        private KumiPayment ApplyMemorialOrder(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            AddPaymentCorrections(payment, memorialOrder);
            if (memorialOrder.SumZach < 0)
            {
                if (payment.Kbk != memorialOrder.Kbk || payment.IdKbkType != memorialOrder.IdKbkType || payment.TargetCode != memorialOrder.TargetCode ||
                    payment.Okato != memorialOrder.Okato || payment.PayerInn != memorialOrder.InnAdb || payment.PayerKpp != memorialOrder.KppAdb)
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
            payment.PayerInn = memorialOrder.InnAdb;
            payment.PayerKpp = memorialOrder.KppAdb;

            // Связываем мемориальные ордера с платежом
            if (payment.MemorialOrderPaymentAssocs == null)
            {
                payment.MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc>();
            }
            payment.MemorialOrderPaymentAssocs.Add(new KumiMemorialOrderPaymentAssoc { Order = memorialOrder });

            return payment;
        }

        private void AddPaymentCorrections(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            var date = DateTime.Now;
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
            if (payment.PayerInn != memorialOrder.InnAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "PayerInn",
                    FieldValue = payment.PayerInn,
                    Date = date
                });
            }
            if (payment.PayerKpp != memorialOrder.KppAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "PayerKpp",
                    FieldValue = payment.PayerKpp,
                    Date = date
                });
            }
        }

        private KumiPayment ApplyPaymentExtract(KumiPayment payment, KumiPaymentExtract extract)
        {
            if (kumiPaymentDocCodes == null)
                kumiPaymentDocCodes = registryContext.KumiPaymentDocCodes.ToList();
            if (extract.Guid != payment.Guid)
                throw new KumiPaymentCheckVtOperException(string.Format("Попытка привязать к платежу {0} строку выписки {1}", payment.Guid, extract.Guid));

            if (extract.SumIn != payment.Sum)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие суммы в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.Sum, extract.Guid, extract.SumIn));
            if (extract.NumDoc != payment.NumDocument)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие номера платежного документа в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.NumDocument, extract.Guid, extract.NumDoc));
            if (extract.DateDoc != payment.DateDocument)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие даты платежного документа в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.DateDocument?.ToString("dd.MM.yyyy"), extract.Guid, extract.DateDoc.ToString("dd.MM.yyyy")));
            var docCode = kumiPaymentDocCodes.FirstOrDefault(r => r.Code == extract.CodeDoc);
            if (docCode == null)
                throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный код документа, подтверждающего проведение операции {0} в выписке по платежу {1}", 
                    extract.CodeDoc, extract.Guid));
            payment.IdPaymentDocCode = docCode.IdPaymentDocCode;
            return payment;
        }

        // Buffer
        private List<KumiPaymentKind> kumiPaymentKinds { get; set; }
        private List<KumiOperationType> kumiOperationTypes { get; set; }
        private List<KumiKbkType> kumiKbkTypes { get; set; }
        private List<KumiPaymentReason> kumiPaymentReasons { get; set; }
        private List<KumiPayerStatus> kumiPayerStatuses { get; set; }

        private KumiPayment BindDictionariesToPayment(KumiPayment payment)
        {
            if (kumiPaymentKinds == null)
                kumiPaymentKinds = registryContext.KumiPaymentKinds.ToList();
            if (payment.PaymentKind != null)
            {
                var paymentKind = kumiPaymentKinds.FirstOrDefault(r => r.Code == payment.PaymentKind.Code);
                if (paymentKind == null)
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид платежа {0} в платеже {1}", payment.OperationType.Code, payment.Guid));
                payment.PaymentKind = null;
                payment.IdPaymentKind = paymentKind.IdPaymentKind;
            }

            if (kumiOperationTypes == null)
                kumiOperationTypes = registryContext.KumiOperationTypes.ToList();
            if (payment.OperationType != null)
            {
                var operationType = kumiOperationTypes.FirstOrDefault(r => r.Code == payment.OperationType.Code);
                if (operationType == null && payment.OperationType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид операции {0} в платеже {1}", payment.OperationType.Code, payment.Guid));
                payment.OperationType = null;
                payment.IdOperationType = operationType?.IdOperationType;
            }

            if (kumiKbkTypes == null)
                kumiKbkTypes = registryContext.KumiKbkTypes.ToList();
            if (payment.KbkType != null)
            {
                var kbkType = kumiKbkTypes.FirstOrDefault(r => r.Code == payment.KbkType.Code);
                if (kbkType == null && payment.KbkType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный тип КБК {0} в платеже {1}", payment.KbkType.Code, payment.Guid));
                payment.KbkType = null;
                payment.IdKbkType = kbkType?.IdKbkType;
            }

            if (kumiPaymentReasons == null)
                kumiPaymentReasons = registryContext.KumiPaymentReasons.ToList();
            if (payment.PaymentReason != null)
            {
                var paymentReason = kumiPaymentReasons.FirstOrDefault(r => r.Code == payment.PaymentReason.Code);
                if (paymentReason == null && payment.PaymentReason.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PaymentReason.Code, payment.Guid));
                payment.PaymentReason = null;
                payment.IdPaymentReason = paymentReason?.IdPaymentReason;
            }

            if (kumiPayerStatuses == null)
                kumiPayerStatuses = registryContext.KumiPayerStatuses.ToList();
            if (payment.PayerStatus != null)
            {
                var payerStatus = kumiPayerStatuses.FirstOrDefault(r => r.Code == payment.PayerStatus.Code);
                if (payerStatus == null && payment.PayerStatus.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PayerStatus.Code, payment.Guid));
                payment.PayerStatus = null;
                payment.IdPayerStatus = payerStatus?.IdPayerStatus;
            }
            return payment;
        }

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
    }
}
