using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    class KumiAccountAddressInfixConfiguration : IEntityTypeConfiguration<KumiAccountAddressInfix>
    {
        private string nameDatebase;

        public KumiAccountAddressInfixConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccountAddressInfix> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("kumi_accounts_address_infix", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Infix)
                .HasColumnName("infix")
                .HasColumnType("varchar(256)")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.Address)
                .HasColumnName("address")
                .HasColumnType("varchar(1024)")
                .HasMaxLength(1024)
                .IsRequired();

            builder.Property(e => e.TotalArea)
                .HasColumnName("total_area")
                .HasColumnType("double");
        }
    }
}
