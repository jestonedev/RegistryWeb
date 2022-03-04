using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiOperationTypeConfiguration : IEntityTypeConfiguration<KumiOperationType>
    {
        private string nameDatebase;

        public KumiOperationTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiOperationType> builder)
        {
            builder.HasKey(e => e.IdOperationType);

            builder.ToTable("kumi_operation_types", nameDatebase);

            builder.Property(e => e.IdOperationType)
                .HasColumnName("id_operation_type")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false);
        }
    }
}
