using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiPaymentGroupFileConfiguration : IEntityTypeConfiguration<KumiPaymentGroupFile>
    {
        private string nameDatebase;

        public KumiPaymentGroupFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiPaymentGroupFile> builder)
        {
            builder.HasKey(e => e.IdFile);

            builder.ToTable("kumi_payment_group_files", nameDatebase);

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdGroup)
                .HasColumnName("id_group")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FileVersion)
                .HasColumnName("file_version")
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);
        }
    }
}
