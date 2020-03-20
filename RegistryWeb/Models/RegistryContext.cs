using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models.IEntityTypeConfiguration;

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
        public virtual DbSet<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual DbSet<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual DbSet<OwnershipRightType> OwnershipRightTypes { get; set; }
        public virtual DbSet<OwnershipRight> OwnershipRights { get; set; }

        public virtual DbSet<RestrictionBuildingAssoc> RestrictionBuildingsAssoc { get; set; }
        public virtual DbSet<RestrictionPremiseAssoc> RestrictionPremisesAssoc { get; set; }
        public virtual DbSet<RestrictionType> RestrictionTypes { get; set; }
        public virtual DbSet<Restriction> Restrictions { get; set; }

        public virtual DbSet<PremisesComment> PremisesComments { get; set; }
        public virtual DbSet<PremisesDoorKeys> PremisesDoorKeys { get; set; }
        public virtual DbSet<PremisesKind> PremisesKinds { get; set; }
        public virtual DbSet<PremisesType> PremisesTypes { get; set; }
        
        public virtual DbSet<StructureType> StructureTypes { get; set; }

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
        public virtual DbSet<TenancyReason> TenancyReasons { get; set; }
        public virtual DbSet<RentTypeCategory> RentTypeCategories { get; set; }
        public virtual DbSet<Kinship> Kinships { get; set; }
        public virtual DbSet<RentType> RentTypes { get; set; }
        public virtual DbSet<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual DbSet<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }
        public virtual DbSet<TenancySubPremiseAssoc> TenancySubPremisesAssoc { get; set; }

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

        public virtual DbSet<PersonalSetting> PersonalSettings { get; set; }

        //SQL-Views
        public virtual DbSet<KladrStreet> KladrStreets { get; set; }
        public virtual DbSet<TenancyActiveProcess> TenancyActiveProcesses { get; set; }
        public virtual DbSet<OwnerActiveProcess> OwnerActiveProcesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            //SQL-Views
            modelBuilder.ApplyConfiguration(new KladrStreetConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyActiveProcessConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerActiveProcessConfiguration(nameDatebase));

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

            modelBuilder.ApplyConfiguration(new PersonalSettingConfiguration(nameDatebase)); 

            modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DocumentIssuedByConfiguration(nameDatebase));
            
            modelBuilder.ApplyConfiguration(new BuildingConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new SubPremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ObjectStateConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new HeatingTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesCommentConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesDoorKeysConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new OwnershipRightConfiguration(nameDatebase));            
            modelBuilder.ApplyConfiguration(new OwnershipBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipRightTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new RestrictionConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RestrictionTypeConfiguration(nameDatebase));

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
            modelBuilder.ApplyConfiguration(new TenancyReasonConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RentTypeCategoryConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new RentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancyPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new TenancySubPremiseAssocConfiguration(nameDatebase));

            modelBuilder.Entity<DocumentResidence>(entity =>
            {
                entity.HasKey(e => e.IdDocumentResidence);

                entity.ToTable("documents_residence", nameDatebase);

                entity.Property(e => e.IdDocumentResidence)
                    .HasColumnName("id_document_residence")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.DocumentResidenceName)
                    .IsRequired()
                    .HasColumnName("document_residence")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            modelBuilder.Entity<PremisesKind>(entity =>
            {
                entity.HasKey(e => e.IdPremisesKind);

                entity.ToTable("premises_kinds", nameDatebase);

                entity.Property(e => e.IdPremisesKind)
                    .HasColumnName("id_premises_kind")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PremisesKindName)
                    .HasColumnName("premises_kind")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PremisesType>(entity =>
            {
                entity.HasKey(e => e.IdPremisesType);

                entity.ToTable("premises_types", nameDatebase);

                entity.Property(e => e.IdPremisesType)
                    .HasColumnName("id_premises_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PremisesTypeName)
                    .IsRequired()
                    .HasColumnName("premises_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PremisesTypeAsNum)
                    .HasColumnName("premises_type_as_num")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PremisesTypeShort)
                    .HasColumnName("premises_type_short")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<StructureType>(entity =>
            {
                entity.HasKey(e => e.IdStructureType);

                entity.ToTable("structure_types", nameDatebase);

                entity.Property(e => e.IdStructureType)
                    .HasColumnName("id_structure_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.StructureTypeName)
                    .IsRequired()
                    .HasColumnName("structure_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });
        }
    }
}