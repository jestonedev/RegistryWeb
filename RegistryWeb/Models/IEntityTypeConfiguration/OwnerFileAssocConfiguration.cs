using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerFileAssocConfiguration : IEntityTypeConfiguration<OwnerFileAssoc>
    {
        private string nameDatebase;

        public OwnerFileAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerFileAssoc> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("owner_files_assoc", nameDatebase);

            builder.HasIndex(e => e.IdOwner)
                .HasName("FK_owner_files_assoc_id_owner");

            builder.HasIndex(e => e.IdFile)
                .HasName("FK_owner_files_assoc_id_file");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.NumeratorShare)
                .HasColumnName("numerator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.DenominatorShare)
                .HasColumnName("denominator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.Owner)
                .WithMany(ow => ow.OwnerFilesAssoc)
                .HasForeignKey(e => e.IdOwner)
                .HasConstraintName("FK_owner_files_assoc_id_owner");

            builder.HasOne(e => e.OwnerFile)
                .WithMany(of => of.OwnerFilesAssoc)
                .HasForeignKey(e => e.IdFile)
                .HasConstraintName("FK_owner_files_assoc_id_file");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
