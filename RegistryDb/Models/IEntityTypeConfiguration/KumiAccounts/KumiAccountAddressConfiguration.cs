using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiAccountAddressConfiguration : IEntityTypeConfiguration<KumiAccountAddress>
    {
        private string nameDatebase;

        public KumiAccountAddressConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }
        public void Configure(EntityTypeBuilder<KumiAccountAddress> builder)
        {
            builder.HasKey(e => e.IdProcess);
            builder.ToTable("v_kumi_account_address", nameDatebase);
           /* builder.HasIndex(e => e.IdProcess)
                .HasName("IDX_v_kumi_account_address_id_process");*/

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)");

            builder.Property(e => e.Address)
                .HasColumnName("address")
                .IsUnicode(false);

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(30)");

           builder.HasOne(d => d.KumiAccountNavigation)
               .WithOne(p => p.KumiAccountAddressNavigation)
               .HasForeignKey<KumiAccountAddress>(d => d.IdAccount);
           
            /*
            builder.HasMany(d => d.TenancyPersons)
                .WithOne(c => c.IdKumiAccountAddressNavigation)
                .HasForeignKey(d => d.IdProcess); */
        }

    }
}
