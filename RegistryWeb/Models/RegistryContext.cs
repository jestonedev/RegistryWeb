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

        public virtual DbSet<Buildings> Buildings { get; set; }
        public virtual DbSet<DocumentTypes> DocumentTypes { get; set; }
        public virtual DbSet<DocumentsIssuedBy> DocumentsIssuedBy { get; set; }
        public virtual DbSet<DocumentsResidence> DocumentsResidence { get; set; }
        public virtual DbSet<FundTypes> FundTypes { get; set; }
        public virtual DbSet<FundsBuildingsAssoc> FundsBuildingsAssoc { get; set; }
        public virtual DbSet<FundsHistory> FundsHistory { get; set; }
        public virtual DbSet<FundsPremisesAssoc> FundsPremisesAssoc { get; set; }
        public virtual DbSet<FundsSubPremisesAssoc> FundsSubPremisesAssoc { get; set; }
        public virtual DbSet<HeatingType> HeatingType { get; set; }
        public virtual DbSet<Kinships> Kinships { get; set; }
        public virtual DbSet<ObjectStates> ObjectStates { get; set; }
        public virtual DbSet<OwnerBuildingsAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual DbSet<OwnerPersons> OwnerPersons { get; set; }
        public virtual DbSet<OwnerPremisesAssoc> OwnerPremisesAssoc { get; set; }
        public virtual DbSet<OwnerProcesses> OwnerProcesses { get; set; }
        public virtual DbSet<OwnerReasonTypes> OwnerReasonTypes { get; set; }
        public virtual DbSet<OwnerReasons> OwnerReasons { get; set; }
        public virtual DbSet<OwnerSubPremisesAssoc> OwnerSubPremisesAssoc { get; set; }
        public virtual DbSet<OwnerType> OwnerType { get; set; }
        public virtual DbSet<OwnershipBuildingsAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual DbSet<OwnershipPremisesAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual DbSet<OwnershipRightTypes> OwnershipRightTypes { get; set; }
        public virtual DbSet<OwnershipRights> OwnershipRights { get; set; }
        public virtual DbSet<Premises> Premises { get; set; }
        public virtual DbSet<PremisesComments> PremisesComments { get; set; }
        public virtual DbSet<PremisesKinds> PremisesKinds { get; set; }
        public virtual DbSet<PremisesTypes> PremisesTypes { get; set; }
        public virtual DbSet<RentTypeCategories> RentTypeCategories { get; set; }
        public virtual DbSet<RentTypes> RentTypes { get; set; }
        public virtual DbSet<StructureTypes> StructureTypes { get; set; }
        public virtual DbSet<SubPremises> SubPremises { get; set; }

        //SQL-Views
        public virtual DbSet<KladrStreets> KladrStreets { get; set; }

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

            modelBuilder.ApplyConfiguration(new BuildingsConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new PremisesConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new SubPremisesConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new HeatingTypeConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new OwnershipRightsConfiguration(NameDatebase));            
            modelBuilder.ApplyConfiguration(new OwnershipBuildingsAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipPremisesAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnershipRightTypesConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new FundsBuildingsAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundsPremisesAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundsSubPremisesAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundsHistoryConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new FundTypesConfiguration(NameDatebase));

            modelBuilder.ApplyConfiguration(new OwnerBuildingsAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPremisesAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerSubPremisesAssocConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerPersonsConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerProcessesConfiguration(NameDatebase));
            modelBuilder.ApplyConfiguration(new OwnerReasonsConfiguration(NameDatebase));

            modelBuilder.Entity<DocumentTypes>(entity =>
            {
                entity.HasKey(e => e.IdDocumentType);

                entity.ToTable("document_types", NameDatebase);

                entity.Property(e => e.IdDocumentType)
                    .HasColumnName("id_document_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DocumentType)
                    .IsRequired()
                    .HasColumnName("document_type")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DocumentsIssuedBy>(entity =>
            {
                entity.HasKey(e => e.IdDocumentIssuedBy);

                entity.ToTable("documents_issued_by", NameDatebase);

                entity.Property(e => e.IdDocumentIssuedBy)
                    .HasColumnName("id_document_issued_by")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.DocumentIssuedBy)
                    .IsRequired()
                    .HasColumnName("document_issued_by")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            modelBuilder.Entity<DocumentsResidence>(entity =>
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

                entity.Property(e => e.DocumentResidence)
                    .IsRequired()
                    .HasColumnName("document_residence")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            modelBuilder.Entity<Kinships>(entity =>
            {
                entity.HasKey(e => e.IdKinship);

                entity.ToTable("kinships", NameDatebase);

                entity.Property(e => e.IdKinship)
                    .HasColumnName("id_kinship")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Kinship)
                    .IsRequired()
                    .HasColumnName("kinship")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ObjectStates>(entity =>
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

            modelBuilder.Entity<OwnerReasonTypes>(entity =>
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

            modelBuilder.Entity<PremisesComments>(entity =>
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

            modelBuilder.Entity<PremisesKinds>(entity =>
            {
                entity.HasKey(e => e.IdPremisesKind);

                entity.ToTable("premises_kinds", NameDatebase);

                entity.Property(e => e.IdPremisesKind)
                    .HasColumnName("id_premises_kind")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PremisesKind)
                    .HasColumnName("premises_kind")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PremisesTypes>(entity =>
            {
                entity.HasKey(e => e.IdPremisesType);

                entity.ToTable("premises_types", NameDatebase);

                entity.Property(e => e.IdPremisesType)
                    .HasColumnName("id_premises_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PremisesType)
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

            modelBuilder.Entity<RentTypeCategories>(entity =>
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

                entity.Property(e => e.RentTypeCategory)
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

            modelBuilder.Entity<RentTypes>(entity =>
            {
                entity.HasKey(e => e.IdRentType);

                entity.ToTable("rent_types", NameDatebase);

                entity.Property(e => e.IdRentType)
                    .HasColumnName("id_rent_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RentType)
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

            modelBuilder.Entity<StructureTypes>(entity =>
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

                entity.Property(e => e.StructureType)
                    .IsRequired()
                    .HasColumnName("structure_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                //Фильтры по умолчанию
                entity.HasQueryFilter(e => e.Deleted == 0);
            });

            //SQL-Views
            modelBuilder.Entity<KladrStreets>(entity =>
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