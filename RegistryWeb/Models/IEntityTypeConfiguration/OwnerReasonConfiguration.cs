using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerReasonConfiguration : IEntityTypeConfiguration<OwnerReason>
    {
        private string nameDatebase;

        public OwnerReasonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerReason> builder)
        {
            builder.HasKey(e => e.IdReason);

            builder.ToTable("owner_reasons", nameDatebase);

            builder.HasIndex(e => e.IdOwner)
                .HasName("FK_owner_reasons_id_orginfo"); ;

            builder.HasIndex(e => e.IdOwnerType)
                .HasName("FK_owner_reasons_id_owner_type");

            builder.HasIndex(e => e.IdReasonType)
                .HasName("FK_owner_reasons_id_reason_type");

            builder.Property(e => e.IdReason)
                .HasColumnName("id_reason")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnerType)
                .HasColumnName("id_owner_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.NumeratorShare)
                .HasColumnName("numerator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.DenominatorShare)
                .HasColumnName("denominator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonNumber)
                .HasColumnName("reason_number")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ReasonDate)
                .HasColumnName("reason_date")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOrginfoNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdOwner)
                .HasConstraintName("FK_owner_reasons_id_orginfo");

            builder.HasOne(d => d.IdPersonNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdOwner)
                .HasConstraintName("FK_owner_reasons_id_person");

            builder.HasOne(d => d.IdOwnerTypeNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdOwnerType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_reasons_id_owner_type");

            builder.HasOne(d => d.IdReasonTypeNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdReasonType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_reasons_id_reason_type");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
