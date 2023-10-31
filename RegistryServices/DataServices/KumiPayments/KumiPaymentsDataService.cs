using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using RegistryServices.ViewModel.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.TffStrings;
using RegistryPaymentsLoader.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.SqlViews;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.Models.KumiPayments;
using System.Text.Json;
using System.Text.Json.Serialization;
using RegistryServices.Classes;
using RegistryServices.DataFilterServices;
using RegistryWeb.DataServices;
using System.Text.RegularExpressions;
using RegistryServices.DataServices.KumiAccounts;

namespace RegistryServices.DataServices.KumiPayments
{
    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        private readonly KumiAccountsDataService kumiAccountsDataService;
        private readonly KumiPaymentsDistributionsService distributionsService;
        private readonly KumiPaymentsMemorialOrdersService memorialOrdersService;
        private readonly SecurityService securityService;
        private readonly IFilterService<KumiPayment, KumiPaymentsFilter> filterService;

        public KumiPaymentsDataService(
            RegistryContext registryContext, KumiAccountsDataService kumiAccountsDataService,
            KumiPaymentsDistributionsService distributionsService,
            KumiPaymentsMemorialOrdersService memorialOrdersService,
            AddressesDataService addressesDataService, SecurityService securityService,
            FilterServiceFactory<IFilterService<KumiPayment, KumiPaymentsFilter>> filterServiceFactory) : base(registryContext, addressesDataService)
        {
            this.kumiAccountsDataService = kumiAccountsDataService;
            this.distributionsService = distributionsService;
            this.memorialOrdersService = memorialOrdersService;
            this.securityService = securityService;
            filterService = filterServiceFactory.CreateInstance();
        }

        public KumiPaymentsUploadStateModel UploadInfoFromTff(List<TffString> tffStrings, List<KumiPaymentGroupFile> kumiPaymentGroupFiles,
            int? idParentPayment,
            out int idGroup)
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

            var payments = knownPayments.Union(unknownPayments).Distinct();

            var group = new KumiPaymentGroup {
                Date = DateTime.Now,
                PaymentGroupFiles = kumiPaymentGroupFiles,
                User = securityService.User.UserName
            };
            registryContext.KumiPaymentGroups.Add(group);

            if (idParentPayment != null)
            {
                var parentPayment = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims).FirstOrDefault(r => r.IdPayment == idParentPayment);
                if (parentPayment.PaymentCharges.Any() || parentPayment.PaymentClaims.Any())
                    throw new ApplicationException("Платеж, указанный как сводное платежное поручение частично или полностью распределен. Необходимо сначала отменить распределение выбранного сводного платежного поручения");
                var paymentsTotalSum = payments.Select(r => r.Sum).Sum();
                if (parentPayment == null || parentPayment.Sum != paymentsTotalSum)
                    throw new ApplicationException("Сумма, указанная в сводном платежном поручении не соответствует общей сумме загружаемых платежей");

                foreach(var payment in payments)
                {
                    payment.IdParentPayment = idParentPayment;
                }
                parentPayment.IsConsolidated = 1;
                registryContext.KumiPayments.Update(parentPayment);
            }

            UploadPayments(payments, group, extracts, loadState);
            memorialOrdersService.UploadMemorialOrders(memorialOrders, group, loadState);

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            loadState.AutoDistributedPayments = distributionsService.AutoDistributeUploadedPayments(loadState.InsertedPayments);

            var loadStateSerializeObject = JsonSerializer.Serialize(loadState, typeof(KumiPaymentsUploadStateModel), new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            });

            var log = new KumiPaymentGroupLog()
            {
                IdGroup = group.IdGroup,
                Log = loadStateSerializeObject
            };
            registryContext.KumiPaymentGroupLog.Add(log);

            registryContext.SaveChanges();

            idGroup = log.IdGroup;
            return loadState;
        }

        public override KumiPaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.PaymentSourcesList = new SelectList(registryContext.KumiPaymentInfoSources.ToList(), "IdSource", "Name");
            return vm;
        }

        public KumiPaymentsVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions, out List<int> filteredPaymentsIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = filterService.GetQueryFilter(payments, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;

            filteredPaymentsIds = query.Select(r => r.IdPayment).ToList();

            query = filterService.GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.DistributionInfoToObjects = distributionsService.GetDistributionInfoToObjects(viewModel.Payments.Select(r => r.IdPayment).ToList());
            if (filterOptions?.IdCharge != null)
            {
                var charge = registryContext.KumiCharges.Where(r => r.IdCharge == filterOptions.IdCharge).FirstOrDefault();
                if (charge != null)
                {
                    viewModel.StartDate = charge.StartDate;
                    viewModel.EndDate = charge.EndDate;
                    viewModel.Account = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == charge.IdAccount)?.Account;
                }
            }
            if (filterOptions?.IdAccount != null)
            {
                viewModel.Account = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == filterOptions.IdAccount)?.Account;
            }

            return viewModel;
        }

        private IQueryable<KumiPayment> GetQuery()
        {
            return registryContext.KumiPayments
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup);
        }

        public KumiPayment GetKumiPaymentForApplyMemorialOrder(int idPayment)
        {
            return GetQuery().FirstOrDefault(r => r.IdPayment == idPayment);
        }

        public void UpdateDescription(int idPayment, string description)
        {
            var payment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == idPayment);
            payment.Description = description;
            registryContext.SaveChanges();
        }

        public void UpdateBksPaymentsDateEnrollUfkForm(DateTime dateDoc, DateTime dateEnrollUfk)
        {
            var payments = registryContext.KumiPayments.Where(r => r.DateDocument == dateDoc && r.IdSource == 6);
            foreach(var payment in payments)
            {
                payment.DateEnrollUfk = dateEnrollUfk;
            }
            registryContext.SaveChanges();
        }

        private void UploadPayments(IEnumerable<KumiPayment> payments, KumiPaymentGroup group, List<KumiPaymentExtract> extracts, KumiPaymentsUploadStateModel loadState)
        {
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
                        case 6:
                        case 7:
                            loadState.KnownPayments.Add(paymentLocal);
                            break;
                        case 8:
                            loadState.ReturnPayments.Add(paymentLocal);
                            break;
                        default:
                            throw new KumiPaymentBindDictionaryException(string.Format("Неподдерживаемый источник платежа {0} - {1}", payment.Guid, payment.IdSource));
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.PaymentsDicitionaryBindErrors.Add(new RegistryTuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.CheckExtractErrors.Add(new RegistryTuple<KumiPayment, string>(payment, e.Message));
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
                            dbPayment.MemorialOrderPaymentAssocs.Any() || dbPayment.PaymentUfs.Any() || dbPayment.IsConsolidated == 1)
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

        private KumiPayment ApplyPaymentExtract(KumiPayment payment, KumiPaymentExtract extract)
        {
            if (kumiPaymentDocCodes == null)
                kumiPaymentDocCodes = registryContext.KumiPaymentDocCodes.ToList();
            if (extract.Guid != payment.Guid)
                throw new KumiPaymentCheckVtOperException(string.Format("Попытка привязать к платежу {0} строку выписки {1}", payment.Guid, extract.Guid));

            if (extract.CodeDocAdp == "ZV" && extract.SumOut == payment.Sum)
            {
                payment.Sum = -payment.Sum;
                payment.IdSource = 8;
            }
            else if (extract.SumIn != payment.Sum)
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
                if (paymentKind == null && payment.PaymentKind.Code != "")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид платежа {0} в платеже {1}", payment.PaymentKind.Code, payment.Guid));
                payment.PaymentKind = null;
                payment.IdPaymentKind = paymentKind?.IdPaymentKind;
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
                if (paymentReason == null && payment.PaymentReason.Code != "0" && payment.PaymentReason.Code != "00")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PaymentReason.Code, payment.Guid));
                payment.PaymentReason = null;
                payment.IdPaymentReason = paymentReason?.IdPaymentReason;
            }

            if (kumiPayerStatuses == null)
                kumiPayerStatuses = registryContext.KumiPayerStatuses.ToList();
            if (payment.PayerStatus != null)
            {
                var payerStatus = kumiPayerStatuses.FirstOrDefault(r => r.Code == payment.PayerStatus.Code);
                if (payerStatus == null && payment.PayerStatus.Code != "0" && payment.PayerStatus.Code != "00")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный статус составителя расчетного документа {0} в платеже {1}", payment.PayerStatus.Code, payment.Guid));
                payment.PayerStatus = null;
                payment.IdPayerStatus = payerStatus?.IdPayerStatus;
            }
            return payment;
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
            foreach(var assoc in payment.MemorialOrderPaymentAssocs)
            {
                assoc.Order = registryContext.KumiMemorialOrders.FirstOrDefault(r => r.IdOrder == assoc.IdOrder);
                assoc.IdOrder = assoc.Order?.IdOrder ?? 0;
            }
            foreach (var assoc in payment.PaymentCharges)
            {
                assoc.Charge = registryContext.KumiCharges.Include(r => r.Account).FirstOrDefault(r => r.IdCharge == assoc.IdCharge);
                assoc.IdCharge = assoc.Charge?.IdCharge ?? 0;
            }
            foreach (var assoc in payment.PaymentClaims)
            {
                assoc.Claim = registryContext.Claims.Include(r => r.IdAccountKumiNavigation).FirstOrDefault(r => r.IdClaim == assoc.IdClaim);
                assoc.IdClaim = assoc.Claim?.IdClaim ?? 0;
            }
            return payment;
        }

        public SelectableSigner GetSigner(int idSigner)
        {
            return registryContext.SelectableSigners.FirstOrDefault(r => r.IdRecord == idSigner);
        }

        public KumiPaymentSettingSet GetKumiPaymentSettings()
        {
            return registryContext.KumiPaymentSettingSets.FirstOrDefault();
        }

        public Dictionary<int, string> GetAccountsTenants(IEnumerable<KumiAccount> accounts)
        {
            var tenants = registryContext.GetTenantsByAccountIds(accounts.Select(r => r.IdAccount).ToList());
            var result = new Dictionary<int, string>();
            foreach (var account in accounts)
            {
                var tenant = account.Owner;
                if (string.IsNullOrWhiteSpace(tenant))
                    tenant = tenants.FirstOrDefault(r => r.IdAccount == account.IdAccount)?.Tenant;
                if (!result.ContainsKey(account.IdAccount))
                    result.Add(account.IdAccount, tenant);
            }
            return result;
        }

        public List<KumiPayment> SearchPaymentsRaw(string text)
        {
            var terms = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            DateTime? docDate = null;
            decimal? sum = null;
            string docNum = null;
            foreach (var term in terms)
            {
                var termPrepared = term.Trim(new char[] { '№', '#', '.', ',' });
                if (termPrepared.Length == 10 && new Regex(@"^[0-9]{2}\.[0-9]{2}\.[0-9]{4}$").IsMatch(termPrepared))
                {
                    // Дата документа
                    var dateParts = termPrepared.Split('.');
                    docDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                }
                else
                if (int.TryParse(termPrepared, out int digBuffer))
                {
                    // Номер документа
                    docNum = digBuffer.ToString();
                }
                else
                if (decimal.TryParse(termPrepared.Replace(".", ","), out decimal sumBuffer))
                {
                    // Сумма платежа
                    sum = sumBuffer;
                }
            }
            if (docDate == null && sum == null && string.IsNullOrEmpty(docNum)) return new List<KumiPayment>();
            return registryContext.KumiPayments.Where(r =>
                (docNum == null || r.NumDocument == docNum) &&
                (docDate == null || r.DateDocument == docDate) &&
                (sum == null || r.Sum == sum)).Take(10).ToList();
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
                .Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(pc => pc.IdPayment == idPayment);
            payments.Deleted = 1;
            foreach(var mo in payments.MemorialOrderPaymentAssocs)
            {
                registryContext.KumiMemorialOrderPaymentAssocs.Remove(mo);
            }
            registryContext.SaveChanges();
        }

        public List<KumiPaymentGroup> PaymentGroups { get => registryContext.KumiPaymentGroups.ToList(); }
        public List<KumiPaymentInfoSource> PaymentInfoSources { get => registryContext.KumiPaymentInfoSources.ToList(); }
        public List<KumiPaymentDocCode> PaymentDocCodes { get => registryContext.KumiPaymentDocCodes.ToList(); }
        public List<KumiPaymentKind> PaymentKinds { get => registryContext.KumiPaymentKinds.ToList(); }
        public List<KumiOperationType> OperationTypes { get => registryContext.KumiOperationTypes.ToList(); }
        public List<KumiKbkType> KbkTypes { get => registryContext.KumiKbkTypes.ToList(); }
        public List<KumiKbkDescription> KbkDescriptions { get => registryContext.KumiKbkDescriptions.ToList(); }
        public List<KumiPaymentReason> PaymentReasons { get => registryContext.KumiPaymentReasons.ToList(); }
        public List<KumiPayerStatus> PayerStatuses { get => registryContext.KumiPayerStatuses.ToList(); }
        public List<SelectableSigner> PaymentUfSigners { get => registryContext.SelectableSigners.Where(r => r.IdSignerGroup == 5).ToList(); }
        public List<KladrRegion> Regions { get => registryContext.KladrRegions.ToList(); }
        public List<KladrStreet> Streets { get => registryContext.KladrStreets.ToList(); }
        public List<KumiAccountState> AccountStates { get => registryContext.KumiAccountStates.ToList(); }
        public List<ClaimStateType> ClaimStateTypes { get => registryContext.ClaimStateTypes.ToList(); }

        public KumiPaymentsVM GetKumiPaymentViewModelForMassDistribution(List<int> ids)
        {
            var viewModel = InitializeViewModel(null,null, null);
            viewModel.Payments = GetPaymentsForMassDistribution(ids).ToList();
            viewModel.DistributionInfoToObjects = distributionsService.GetDistributionInfoToObjects(viewModel.Payments.Select(r => r.IdPayment).ToList());
            return viewModel;
        }

        public IQueryable<KumiPayment> GetPaymentsForMassDistribution(List<int> ids)
        {
            return registryContext.KumiPayments
                .Where(b => ids.Contains(b.IdPayment));
        }

        public KumiPaymentGroupsVM GetPaymentGroupsVM(PageOptions pageOptions)
        {
            var vm = new KumiPaymentGroupsVM();
            vm.PageOptions = pageOptions ?? vm.PageOptions;
            var query = registryContext.KumiPaymentGroups
                .Include(r => r.PaymentGroupFiles).OrderByDescending(c=> c.Date).ThenByDescending(c=> c.IdGroup);
            vm.PageOptions.TotalRows = query.Count();
            var count = query.Count();
            vm.PageOptions.Rows = count;
            vm.PageOptions.TotalPages = Math.Max((int)Math.Ceiling(count / (double)vm.PageOptions.SizePage), 1);
            vm.PageOptions.CurrentPage = Math.Min(vm.PageOptions.CurrentPage, vm.PageOptions.TotalPages);

            vm.paymentGroups = GetKumiPaymentGroupPage(query, vm.PageOptions).ToList();

            return vm ;
        }

        private IQueryable<KumiPaymentGroup> GetKumiPaymentGroupPage(IQueryable<KumiPaymentGroup> query, PageOptions pageOptions)
        {
            var page = query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
            return page;
        }

        public KumiPaymentsUploadStateModel UploadLogPaymentGroups(int idGroup)
        {
            var log = registryContext.KumiPaymentGroupLog.FirstOrDefault(c => c.IdGroup == idGroup).Log;

            var loadState = JsonSerializer.Deserialize<KumiPaymentsUploadStateModel>(log, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            });
            return loadState;
        }
    }
}
