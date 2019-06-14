using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models.IEntityTypeConfiguration;

namespace RegistryWeb.Models
{
    public partial class RegistryContext : DbContext
    {
        private string NameDatebase = "registry_test";

        public RegistryContext()
        {
        }

        public RegistryContext(DbContextOptions<RegistryContext> options)
            : base(options)
        {
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

        //SQL-Views
        public virtual DbSet<KladrStreet> KladrStreets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            //    optionsBuilder.UseMySQL("");
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new DocumentIssuedByConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new BuildingConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new PremiseConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new SubPremiseConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new HeatingTypeConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new OwnershipRightConfiguration(NameDatebase));            
            modelBuilder.ApplyConfiguration(new OwnershipBuildingAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipPremiseAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipRightTypeConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new FundBuildingAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundPremiseAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundSubPremiseAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundHistoryConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundTypeConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new OwnerBuildingAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPremiseAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerSubPremiseAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerOrginfoConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPersonConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerProcessConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerReasonConfiguration(NameDatebase));

            modelBuilder.Entity<DocumentResidence>(entity =>
            {
                entity.HasKey(e => e.IdDocumentResidence);

                entity.ToTable("documents_residence", NameDatebase);

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

                entity.ToTable("kinships", NameDatebase);

                entity.Property(e => e.IdKinship)
                    .HasColumnName("id_kinship")
                    .HasColumnType("int(11)");

                entity.Property(e => e.KinshipName)
                    .IsRequired()
                    .HasColumnName("kinship")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ObjectState>(entity =>
            {
                entity.HasKey(e => e.IdState);

                entity.ToTable("object_states", NameDatebase);

                entity.Property(e => e.IdState)
                    .HasColumnName("id_state")
                    .HasColumnType("int(11)");

                entity.Property(e => e.StateFemale)
                    .IsRequired()
                    .HasColumnName("state_female")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.StateNeutral)
                    .IsRequired()
                    .HasColumnName("state_neutral")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OwnerReasonType>(entity =>
            {
                entity.HasKey(e => e.IdReasonType);

                entity.ToTable("owner_reason_types", NameDatebase);

                entity.Property(e => e.IdReasonType)
                    .HasColumnName("id_reason_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.ReasonName)
                    .IsRequired()
                    .HasColumnName("reason_name")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            modelBuilder.Entity<OwnerType>(entity =>
            {
                entity.HasKey(e => e.IdOwnerType);

                entity.ToTable("owner_type", NameDatebase);

                entity.Property(e => e.IdOwnerType)
                    .HasColumnName("id_owner_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.OwnerType1)
                    .IsRequired()
                    .HasColumnName("owner_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            modelBuilder.Entity<PremisesComment>(entity =>
            {
                entity.HasKey(e => e.IdPremisesComment);

                entity.ToTable("premises_comments", NameDatebase);

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

                entity.ToTable("premises_kinds", NameDatebase);

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

                entity.ToTable("premises_types", NameDatebase);

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

                entity.ToTable("rent_type_categories", NameDatebase);

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

                entity.ToTable("rent_types", NameDatebase);

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

                entity.ToTable("structure_types", NameDatebase);

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

                entity.ToTable("v_kladr_streets", NameDatebase);

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