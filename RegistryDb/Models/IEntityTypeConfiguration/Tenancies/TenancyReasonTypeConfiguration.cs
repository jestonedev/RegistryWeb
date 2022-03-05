using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyReasonTypeConfiguration : IEntityTypeConfiguration<TenancyReasonType>
    {
        private string nameDatebase;

        public TenancyReasonTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyReasonType> builder)
        {
            builder.HasKey(e => e.IdReasonType);

            builder.ToTable("tenancy_reason_types", nameDatebase);

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonName)
                .IsRequired()
                .HasColumnName("reason_name")
                .HasMaxLength(150)
                .IsUnicode(false);

            builder.Property(e => e.ReasonTemplate)
                .IsRequired()
                .HasColumnName("reason_template")
                .IsUnicode(false);

            builder.Property(e => e.Order)
                .IsRequired()
                .HasColumnName("order")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
