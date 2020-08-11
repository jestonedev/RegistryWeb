using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models.SqlViews;
using RegistryWeb.Models.IEntityTypeConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RegistryWeb.Models
{
    public partial class RegistryContext : DbContext
    {
        private string nameDatebase;
        private string connString;

        public RegistryContext()
        {
        }

        public RegistryContext(DbContextOptions<RegistryContext> options, IConfiguration config, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            nameDatebase = config.GetValue<string>("Database");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(connString);
            }
        }

        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Premise> Premises { get; set; }
        public virtual DbSet<Preparers> Preparers { get; set; }
        public virtual DbSet<SubPremise> SubPremises { get; set; }
        public virtual DbSet<DocumentType> DocumentTypes { get; set; }
        public virtual DbSet<DocumentIssuedBy> DocumentsIssuedBy { get; set; }
        public virtual DbSet<DocumentResidence> DocumentsResidence { get; set; }
        public virtual DbSet<FundType> FundTypes { get; set; }
        public virtual DbSet<FundBuildingAssoc> FundsBuildingsAssoc { get; set; }
        public virtual DbSet<FundHistory> FundsHistory { get; set; }
        public virtual DbSet<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual DbSet<FundSubPremiseAssoc> FundsSubPremisesAssoc { get; set; }
        public virtual DbSet<HeatingType> HeatingTypes { get; set; }
        public virtual DbSet<ObjectState> ObjectStates { get; set; }        
        public virtual DbSet<TotalAreaAvgCost> TotalAreaAvgCosts { get; set; }
        public virtual DbSet<PremisesComment> PremisesComments { get; set; }
        public virtual DbSet<PremisesDoorKeys> PremisesDoorKeys { get; set; }
        public virtual DbSet<PremisesKind> PremisesKinds { get; set; }
        public virtual DbSet<PremisesType> PremisesTypes { get; set; }
        public virtual DbSet<StructureType> StructureTypes { get; set; }
        public virtual DbSet<StructureTypeOverlap> StructureTypeOverlaps { get; set; }
        public virtual DbSet<FoundationType> FoundationTypes { get; set; }
        public virtual DbSet<GovernmentDecree> GovernmentDecrees { get; set; }
        public virtual DbSet<SelectableSigner> SelectableSigners { get; set; }
        public virtual DbSet<DistrictCommittee> DistrictCommittees { get; set; }
        public virtual DbSet<DistrictCommitteesPreContractPreamble> DistrictCommitteesPreContractPreambles { get; set; }

        //Документы
        public virtual DbSet<ActTypeDocument> ActTypeDocuments { get; set; }
        public virtual DbSet<ActFile> ActFiles { get; set; }
        public virtual DbSet<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
        public virtual DbSet<RestrictionBuildingAssoc> RestrictionBuildingsAssoc { get; set; }
        public virtual DbSet<RestrictionPremiseAssoc> RestrictionPremisesAssoc { get; set; }
        public virtual DbSet<RestrictionType> RestrictionTypes { get; set; }
        public virtual DbSet<Restriction> Restrictions { get; set; }
        public virtual DbSet<Litigation> Litigations { get; set; }
        public virtual DbSet<LitigationPremiseAssoc> LitigationPremisesAssoc { get; set; }
        public virtual DbSet<LitigationType> LitigationTypes { get; set; }
        public virtual DbSet<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual DbSet<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual DbSet<OwnershipRightType> OwnershipRightTypes { get; set; }
        public virtual DbSet<OwnershipRight> OwnershipRights { get; set; }
        public virtual DbSet<ObjectAttachmentFile> ObjectAttachmentFiles { get; set; }
        public virtual DbSet<BuildingAttachmentFileAssoc> BuildingAttachmentFilesAssoc { get; set; }

        //Собственники
        public virtual DbSet<Owner> Owners { get; set; }
        public virtual DbSet<OwnerType> OwnerType { get; set; }
        public virtual DbSet<OwnerOrginfo> OwnerOrginfos { get; set; }
        public virtual DbSet<OwnerPerson> OwnerPersons { get; set; }
        public virtual DbSet<OwnerProcess> OwnerProcesses { get; set; }
        public virtual DbSet<OwnerReasonType> OwnerReasonTypes { get; set; }
        public virtual DbSet<OwnerReason> OwnerReasons { get; set; }
        public virtual DbSet<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual DbSet<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual DbSet<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }

        //Нанимателия        
        public virtual DbSet<TenancyPerson> TenancyPersons { get; set; }
        public virtual DbSet<TenancyProcess> TenancyProcesses { get; set; }
        public virtual DbSet<TenancyRentPeriod> TenancyRentPeriods { get; set; }
        public virtual DbSet<TenancyReason> TenancyReasons { get; set; }
        public virtual DbSet<TenancyAgreement> TenancyAgreements { get; set; }
        public virtual DbSet<TenancyReasonType> TenancyReasonTypes { get; set; }
        public virtual DbSet<RentTypeCategory> RentTypeCategories { get; set; }
        public virtual DbSet<Kinship> Kinships { get; set; }
        public virtual DbSet<RentType> RentTypes { get; set; }
        public virtual DbSet<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual DbSet<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }
        public virtual DbSet<TenancySubPremiseAssoc> TenancySubPremisesAssoc { get; set; }
        public virtual DbSet<Executor> Executors { get; set; }
        public virtual DbSet<TenancyFile> TenancyFiles { get; set; }
        public virtual DbSet<TenancyProlongRentReason> TenancyProlongRentReasons { get; set; }

        // Переселение

        public virtual DbSet<ResettleInfo> ResettleInfos { get; set; }
        public virtual DbSet<ResettleKind> ResettleKinds { get; set; }
        public virtual DbSet<ResettleInfoTo> ResettleInfoTo { get; set; }
        public virtual DbSet<ResettleInfoToFact> ResettleInfoToFact { get; set; }
        public virtual DbSet<ResettleInfoSubPremiseFrom> ResettleInfoSubPremisesFrom { get; set; }
        public virtual DbSet<ResettleDocument> ResettleDocuments { get; set; }
        public virtual DbSet<ResettleDocumentType> ResettleDocumentTypes { get; set; }
        public virtual DbSet<ResettlePremiseAssoc> ResettlePremiseAssoc { get; set; }

        //Журнал изменений
        public virtual DbSet<ChangeLog> ChangeLogs { get; set; }
        public virtual DbSet<LogObject> LogObjects { get; set; }
        public virtual DbSet<LogType> LogTypes { get; set; }
        public virtual DbSet<LogOwnerProcess> LogOwnerProcesses { get; set; }
        public virtual DbSet<LogOwnerProcessValue> LogOwnerProcessesValue { get; set; }

        //Права доступа
        public virtual DbSet<AclPrivilege> AclPrivileges { get; set; }
        public virtual DbSet<AclPrivilegeType> AclPrivilegeTypes { get; set; }
        public virtual DbSet<AclRole> AclRoles { get; set; }
        public virtual DbSet<AclRolePrivilege> AclRolePrivileges { get; set; }
        public virtual DbSet<AclUser> AclUsers { get; set; }
        public virtual DbSet<AclUserPrivilege> AclUserPrivileges { get; set; }
        public virtual DbSet<AclUserRole> AclUserRoles { get; set; }

        //SQL-Views
        public virtual DbSet<KladrStreet> KladrStreets { get; set; }
        public virtual DbSet<TenancyActiveProcess> TenancyActiveProcesses { get; set; }
        public virtual DbSet<TenancyPayment> TenancyPayments { get; set; }
        public virtual DbSet<TenancyPaymentAfter28082019> TenancyPaymentsAfter28082019 { get; set; }
        public virtual DbSet<OwnerActiveProcess> OwnerActiveProcesses { get; set; }
        public virtual DbSet<BuildingOwnershipRightCurrent> BuildingsOwnershipRightCurrent { get; set; }

        public DbQuery<PremiseOwnershipRightCurrent> PremisesOwnershipRightCurrent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            //SQL-Views
            modelBuilder.ApplyConfiguration(new KladrStreetConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyActiveProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyPaymentConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyPaymentAfter28082019Configuration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerActiveProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new BuildingOwnershipRightCurrentConfiguration(nameDatebase));

            modelBuilder.Query<PremiseOwnershipRightCurrent>().ToView("v_premises_ownership_rights_1_current");


            modelBuilder.ApplyConfiguration(new ChangeLogConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LogOwnerProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LogOwnerProcessValueConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LogObjectConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LogTypeConfiguration(nameDatebase));
            
            modelBuilder.ApplyConfiguration(new AclPrivilegeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclPrivilegeTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclRoleConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclRolePrivilegeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclUserConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclUserPrivilegeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new AclUserRoleConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new ActTypeDocumentConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ActFileConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new BuildingDemolitionActFileConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new SelectableSignersConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DistrictCommitteeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DistrictCommitteesPreContractPreambleConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DocumentIssuedByConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DocumentResidenceConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new BuildingConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PreparersConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new SubPremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ObjectStateConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new HeatingTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesCommentConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesDoorKeysConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesKindConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new StructureTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new StructureTypeOverlapConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new FoundationTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new GovernmentDecreeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TotalAreaAvgCostConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new OwnershipRightConfiguration(nameDatebase));            
            modelBuilder.ApplyConfiguration(new OwnershipBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipRightTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new RestrictionConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new LitigationConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LitigationTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new LitigationPremiseAssocConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new ObjectAttachmentFileConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new BuildingAttachmentFileAssocConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new FundBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new FundPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new FundSubPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new FundHistoryConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new FundTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new OwnerBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerSubPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerOrginfoConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPersonConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerReasonConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerReasonTypeConfiguration(nameDatebase));
      
            modelBuilder.ApplyConfiguration(new TenancyPersonConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new KinshipConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyAgreementConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyReasonConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyReasonTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RentTypeCategoryConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancySubPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ExecutorConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyRentPeriodConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyFileConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyProlongRentReasonTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new ResettleInfoConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleKindConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleInfoToConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleInfoToFactConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleInfoSubPremiseFromConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleDocumentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettleDocumentConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ResettlePremiseAssocConfiguration(nameDatebase));

        }
    }
}