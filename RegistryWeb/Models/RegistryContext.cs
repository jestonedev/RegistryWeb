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
        public virtual DbSet<DocumentType> DocumentTypes { get; set; }
        public virtual DbSet<DocumentIssuedBy> DocumentsIssuedBy { get; set; }
        public virtual DbSet<DocumentResidence> DocumentsResidence { get; set; }
        public virtual DbSet<FundType> FundTypes { get; set; }
        public virtual DbSet<FundBuildingAssoc> FundsBuildingsAssoc { get; set; }
        public virtual DbSet<FundHistory> FundsHistory { get; set; }
        public virtual DbSet<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual DbSet<FundSubPremiseAssoc> FundsSubPremisesAssoc { get; set; }
        public virtual DbSet<HeatingType> HeatingTypes { get; set; }
        public virtual DbSet<Kinship> Kinships { get; set; }
        public virtual DbSet<ObjectState> ObjectStates { get; set; }
        public virtual DbSet<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual DbSet<Owner> Owners { get; set; }
        public virtual DbSet<OwnerOrginfo> OwnerOrginfos { get; set; }
        public virtual DbSet<OwnerPerson> OwnerPersons { get; set; }
        public virtual DbSet<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual DbSet<OwnerProcess> OwnerProcesses { get; set; }
        public virtual DbSet<OwnerReasonType> OwnerReasonTypes { get; set; }
        public virtual DbSet<OwnerReason> OwnerReasons { get; set; }
        public virtual DbSet<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }
        public virtual DbSet<OwnerType> OwnerType { get; set; }
        public virtual DbSet<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual DbSet<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual DbSet<OwnershipRightType> OwnershipRightTypes { get; set; }
        public virtual DbSet<OwnershipRight> OwnershipRights { get; set; }
        public virtual DbSet<Premise> Premises { get; set; }
        public virtual DbSet<PremisesComment> PremisesComments { get; set; }
        public virtual DbSet<PremisesKind> PremisesKinds { get; set; }
        public virtual DbSet<PremisesType> PremisesTypes { get; set; }
        public virtual DbSet<RentTypeCategory> RentTypeCategories { get; set; }
        public virtual DbSet<RentType> RentTypes { get; set; }
        public virtual DbSet<StructureType> StructureTypes { get; set; }
        public virtual DbSet<SubPremise> SubPremises { get; set; }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

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

            modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new DocumentIssuedByConfiguration(nameDatebase));
            
            modelBuilder.ApplyConfiguration(new BuildingConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new PremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new SubPremiseConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new ObjectStateConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new HeatingTypeConfiguration(nameDatebase));

            modelBuilder.ApplyConfiguration(new OwnershipRightConfiguration(nameDatebase));            
            modelBuilder.ApplyConfiguration(new OwnershipBuildingAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipPremiseAssocConfiguration(nameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipRightTypeConfiguration(nameDatebase));

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

            modelBuilder.Entity<Kinship>(entity =>
            {
                entity.HasKey(e => e.IdKinship);

                entity.ToTable("kinships", nameDatebase);

                entity.Property(e => e.IdKinship)
                    .HasColumnName("id_kinship")
                    .HasColumnType("int(11)");

                entity.Property(e => e.KinshipName)
                    .IsRequired()
                    .HasColumnName("kinship")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PremisesComment>(entity =>
            {
                entity.HasKey(e => e.IdPremisesComment);

                entity.ToTable("premises_comments", nameDatebase);

                entity.Property(e => e.IdPremisesComment)
                    .HasColumnName("id_premises_comment")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PremisesCommentText)
                    .IsRequired()
                    .HasColumnName("premises_comment_text")
                    .HasMaxLength(255)
                    .IsUnicode(false);
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

            modelBuilder.Entity<RentTypeCategory>(entity =>
            {
                entity.HasKey(e => e.IdRentTypeCategory);

                entity.ToTable("rent_type_categories", nameDatebase);

                entity.HasIndex(e => e.IdRentType)
                    .HasName("FK_rent_type_categories_id_rent_type");

                entity.Property(e => e.IdRentTypeCategory)
                    .HasColumnName("id_rent_type_category")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdRentType)
                    .HasColumnName("id_rent_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RentTypeCategoryName)
                    .IsRequired()
                    .HasColumnName("rent_type_category")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdRentTypeNavigation)
                    .WithMany(p => p.RentTypeCategories)
                    .HasForeignKey(d => d.IdRentType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rent_type_categories_id_rent_type");
            });

            modelBuilder.Entity<RentType>(entity =>
            {
                entity.HasKey(e => e.IdRentType);

                entity.ToTable("rent_types", nameDatebase);

                entity.Property(e => e.IdRentType)
                    .HasColumnName("id_rent_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RentTypeName)
                    .IsRequired()
                    .HasColumnName("rent_type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RentTypeGenetive)
                    .IsRequired()
                    .HasColumnName("rent_type_genetive")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RentTypeShort)
                    .IsRequired()
                    .HasColumnName("rent_type_short")
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

            //SQL-Views
            modelBuilder.Entity<KladrStreet>(entity =>
            {
                entity.HasKey(e => e.IdStreet);

                entity.ToTable("v_kladr_streets", nameDatebase);

                entity.Property(e => e.IdStreet)
                    .HasColumnName("id_street")
                    .HasMaxLength(17)
                    .IsUnicode(false);

                entity.Property(e => e.StreetName)
                    .HasColumnName("street_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.StreetLong)
                    .HasColumnName("street_long")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });
        }
    }
}