using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiKbkTypeConfiguration : IEntityTypeConfiguration<KumiKbkType>
    {
        private string nameDatebase;

        public KumiKbkTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiKbkType> builder)
        {
            builder.HasKey(e => e.IdKbkType);

            builder.ToTable("kumi_kbk_types", nameDatebase);

            builder.Property(e => e.IdKbkType)
                .HasColumnName("id_kbk_type")
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
