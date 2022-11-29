using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Owners;

namespace RegistryDb.Models.IEntityTypeConfiguration.Owners
{
    public class OwnerReasonTypeConfiguration : IEntityTypeConfiguration<OwnerReasonType>
    {
        private string nameDatebase;

        public OwnerReasonTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerReasonType> builder)
        {
            builder.HasKey(e => e.IdReasonType);

            builder.ToTable("owner_reason_types", nameDatebase);

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.ReasonName)
                .IsRequired()
                .HasColumnName("reason_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
