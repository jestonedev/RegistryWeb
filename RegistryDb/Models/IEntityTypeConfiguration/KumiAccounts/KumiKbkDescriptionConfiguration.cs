using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiKbkDescriptionConfiguration : IEntityTypeConfiguration<KumiKbkDescription>
    {
        private string nameDatebase;

        public KumiKbkDescriptionConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiKbkDescription> builder)
        {
            builder.HasKey(e => e.IdKbkDescription);

            builder.ToTable("kumi_kbk_descriptions", nameDatebase);

            builder.Property(e => e.IdKbkDescription)
                .HasColumnName("id_kbk_description")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Kbk)
                .HasColumnName("kbk")
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsRequired()
                .HasMaxLength(1024)
                .IsUnicode(false);
        }
    }
}
