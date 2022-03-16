﻿using RegistryDb.Models;
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
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Text;
using RegistryPaymentsLoader.TffFileLoaders;
using RegistryDb.Models.Entities.Common;

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

        public override KumiPaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.PaymentSourcesList = new SelectList(registryContext.KumiPaymentInfoSources.ToList(), "IdSource", "Name");
            return vm;
        }

        public KumiPaymentsVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = GetQueryFilter(payments, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            return viewModel;
        }

        private IQueryable<KumiPayment> GetQueryOrder(IQueryable<KumiPayment> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPayment);
                else
                    return query.OrderByDescending(p => p.IdPayment);
            }
            return query;
        }

        private IQueryable<KumiPayment> GetQuery()
        {
            return registryContext.KumiPayments
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup);
        }

        private IQueryable<KumiPayment> GetQueryIncludes(IQueryable<KumiPayment> query)
        {
            return query
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments);
        }

        private IQueryable<KumiPayment> GetQueryFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                if (filterOptions.IdParentPayment != null)
                {
                    return query.Where(r => r.IdPayment == filterOptions.IdParentPayment || r.IdParentPayment == filterOptions.IdParentPayment);
                }
                if(!string.IsNullOrWhiteSpace(filterOptions.CommonFilter))
                {
                    return CommonFilter(query, filterOptions.CommonFilter);
                }
                if (!filterOptions.IsModalEmpty())
                {
                    query = CommonPaymentFilter(query, filterOptions);
                    query = PayerFilter(query, filterOptions);
                    query = RecipientFilter(query, filterOptions);
                }
            }
            return query;
        }

        private IQueryable<KumiPayment> RecipientFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.RecipientAccount))
            {
                query = query.Where(r => r.RecipientAccount != null && r.RecipientAccount.Contains(filterOptions.RecipientAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientInn))
            {
                query = query.Where(r => r.RecipientInn != null && r.RecipientInn.Contains(filterOptions.RecipientInn));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientKpp))
            {
                query = query.Where(r => r.RecipientKpp != null && r.RecipientKpp.Contains(filterOptions.RecipientKpp));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientName))
            {
                query = query.Where(r => r.RecipientName != null && r.RecipientName.Contains(filterOptions.RecipientName));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankName))
            {
                query = query.Where(r => r.RecipientBankName != null && r.RecipientName.Contains(filterOptions.RecipientBankName));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankBik))
            {
                query = query.Where(r => r.RecipientBankBik != null && r.RecipientName.Contains(filterOptions.RecipientBankBik));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankAccount))
            {
                query = query.Where(r => r.RecipientBankAccount != null && r.RecipientName.Contains(filterOptions.RecipientBankAccount));
            }
            return query;
        }

        public int? GetKumiPaymentUfsLastNumber()
        {
            var lastNum = registryContext.KumiPaymentUfs.LastOrDefault()?.NumUf;
            if (int.TryParse(lastNum, out int lastNumInt)) return lastNumInt;
            return null;
        }

        public SelectableSigner GetSigner(int idSigner)
        {
            return registryContext.SelectableSigners.FirstOrDefault(r => r.IdRecord == idSigner);
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

        public byte[] GetPaymentUfsFile(List<KumiPaymentUf> paymentUfs, KumiPaymentSettingSet paymentSettings, SelectableSigner signer, DateTime signDate)
        {
            return new TXUF180101FileCreator().CreateFile(paymentUfs, paymentSettings, securityService.Executor, signer, signDate);
        }

        public KumiPaymentSettingSet GetKumiPaymentSettings()
        {
            return registryContext.KumiPaymentSettingSets.FirstOrDefault();
        }

        private IQueryable<KumiPayment> PayerFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if(!string.IsNullOrEmpty(filterOptions.PayerAccount))
            {
                query = query.Where(r => r.PayerAccount != null && r.PayerAccount.Contains(filterOptions.PayerAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerInn))
            {
                query = query.Where(r => r.PayerInn != null && r.PayerInn.Contains(filterOptions.PayerInn));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerKpp))
            {
                query = query.Where(r => r.PayerKpp != null && r.PayerKpp.Contains(filterOptions.PayerKpp));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerName))
            {
                query = query.Where(r => r.PayerName != null && r.PayerName.Contains(filterOptions.PayerName));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankName))
            {
                query = query.Where(r => r.PayerBankName != null && r.PayerName.Contains(filterOptions.PayerBankName));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankBik))
            {
                query = query.Where(r => r.PayerBankBik != null && r.PayerName.Contains(filterOptions.PayerBankBik));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankAccount))
            {
                query = query.Where(r => r.PayerBankAccount != null && r.PayerName.Contains(filterOptions.PayerBankAccount));
            }
            return query;
        }

        private IQueryable<KumiPayment> CommonPaymentFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (filterOptions.IdsSource != null && filterOptions.IdsSource.Any())
            {
                query = query.Where(r => filterOptions.IdsSource.Contains(r.IdSource));
            }
            if (filterOptions.LoadDate != null)
            {
                var endDate = filterOptions.LoadDate.Value.Date.AddDays(1);
                query = query.Where(r => r.PaymentGroup.Date >= filterOptions.LoadDate && r.PaymentGroup.Date < endDate);
            }
            if (filterOptions.IsPosted != null)
            {
                var isPosted = filterOptions.IsPosted.Value ? 1 : 0;
                query = query.Where(r => r.IsPosted == isPosted);
            }
            if (!string.IsNullOrEmpty(filterOptions.NumDocument))
            {
                query = query.Where(r => r.NumDocument != null && r.NumDocument.Contains(filterOptions.NumDocument));
            }
            if (filterOptions.DateDocument != null)
            {
                query = query.Where(r => r.DateDocument == filterOptions.DateDocument);
            }
            if (filterOptions.DateIn != null)
            {
                query = query.Where(r => r.DateIn == filterOptions.DateIn);
            }
            if (filterOptions.DateExecute != null)
            {
                query = query.Where(r => r.DateExecute == filterOptions.DateExecute);
            }
            if (!string.IsNullOrEmpty(filterOptions.Uin))
            {
                query = query.Where(r => r.Uin != null && r.Uin.Contains(filterOptions.Uin));
            }
            if (!string.IsNullOrEmpty(filterOptions.Purpose))
            {
                query = query.Where(r => r.Purpose != null && r.Purpose.Contains(filterOptions.Purpose));
            }
            if (!string.IsNullOrEmpty(filterOptions.Kbk))
            {
                query = query.Where(r => r.Kbk != null && r.Kbk.Contains(filterOptions.Kbk));
            }
            if (!string.IsNullOrEmpty(filterOptions.Okato))
            {
                query = query.Where(r => r.Okato != null && r.Okato.Contains(filterOptions.Okato));
            }
            return query;
        }

        private IQueryable<KumiPayment> CommonFilter(IQueryable<KumiPayment> query, string commonFilter)
        {
            return query.Where(r =>
                (r.Uin != null && r.Uin.Contains(commonFilter)) ||
                (r.Purpose != null && r.Purpose.Contains(commonFilter)) ||
                (r.PayerInn != null && r.PayerInn.Contains(commonFilter)) ||
                (r.PayerKpp != null && r.PayerKpp.Contains(commonFilter)) ||
                (r.PayerName != null && r.PayerName.Contains(commonFilter)) ||
                (r.Kbk != null && r.Kbk.Contains(commonFilter)));
        }

        private IQueryable<KumiPayment> GetQueryPage(IQueryable<KumiPayment> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
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
                            registryContext.KumiPaymentCorrections.Add(correction);
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

        public KumiPayment GetKumiPayment(int idPayment)
        {
            var payment = registryContext.KumiPayments
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup)
                .Include(r => r.PaymentUfs)
                .Include(r => r.PaymentCorrections)
                .Include(r => r.MemorialOrderPaymentAssocs).AsNoTracking()
                .SingleOrDefault(a => a.IdPayment == idPayment);
            return payment;
        }

        public List<KumiMemorialOrder> GetKumiPaymentMemorialOrders(int idPayment)
        {
            var orders = (from order in registryContext.KumiMemorialOrders
                         join assoc in registryContext.KumiMemorialOrderPaymentAssocs
                         on order.IdOrder equals assoc.IdOrder
                         where assoc.IdPayment == idPayment
                         select order).ToList();
            return orders;
        }

        public void Create(KumiPayment payment)
        {
            registryContext.KumiPayments.Add(payment);
            registryContext.SaveChanges();
        }

        public void Edit(KumiPayment payment)
        {
            registryContext.KumiPayments.Update(payment);
            registryContext.SaveChanges();
        }

        public void Delete(int idPayment)
        {
            var payments = registryContext.KumiPayments
                .FirstOrDefault(pc => pc.IdPayment == idPayment);
            payments.Deleted = 1;
            registryContext.SaveChanges();
        }

        public bool AccountExists(string account, int idAccount)
        {
            var curAccount = registryContext.KumiAccounts
                .SingleOrDefault(a => a.IdAccount == idAccount)
                ?.Account;
            if (curAccount == account)
                return false;
            return registryContext.KumiAccounts
                .Select(a => a.Account).Count(num => num != null && num == account) > 0;
        }

        public List<KumiPaymentGroup> PaymentGroups { get => registryContext.KumiPaymentGroups.ToList(); }
        public List<KumiPaymentInfoSource> PaymentInfoSources { get => registryContext.KumiPaymentInfoSources.ToList(); }
        public List<KumiPaymentDocCode> PaymentDocCodes { get => registryContext.KumiPaymentDocCodes.ToList(); }
        public List<KumiPaymentKind> PaymentKinds { get => registryContext.KumiPaymentKinds.ToList(); }
        public List<KumiOperationType> OperationTypes { get => registryContext.KumiOperationTypes.ToList(); }
        public List<KumiKbkType> KbkTypes { get => registryContext.KumiKbkTypes.ToList(); }
        public List<KumiPaymentReason> PaymentReasons { get => registryContext.KumiPaymentReasons.ToList(); }
        public List<KumiPayerStatus> PayerStatuses { get => registryContext.KumiPayerStatuses.ToList(); }
        public List<SelectableSigner> PaymentUfSigners { get => registryContext.SelectableSigners.Where(r => r.IdSignerGroup == 5).ToList(); }
    }
}
