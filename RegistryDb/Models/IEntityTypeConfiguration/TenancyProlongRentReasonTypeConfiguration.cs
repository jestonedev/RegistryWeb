using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class TenancyProlongRentReasonTypeConfiguration : IEntityTypeConfiguration<TenancyProlongRentReason>
    {
        private string nameDatebase;

        public TenancyProlongRentReasonTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyProlongRentReason> builder)
        {
            builder.HasKey(e => e.IdReasonType);

            builder.ToTable("tenancy_prolong_reason_types", nameDatebase);

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonName)
                .IsRequired()
                .HasColumnName("reason_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ReasonTemplateGenetive)
                .IsRequired()
                .HasColumnName("reason_template_genetive")
                .IsUnicode(false);

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
