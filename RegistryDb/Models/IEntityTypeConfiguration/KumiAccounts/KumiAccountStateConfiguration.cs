using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiAccountStateConfiguration : IEntityTypeConfiguration<KumiAccountState>
    {
        private string nameDatebase;

        public KumiAccountStateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccountState> builder)
        {
            builder.HasKey(e => e.IdState);

            builder.ToTable("kumi_accounts_states", nameDatebase);

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)");

            builder.Property(e => e.State)
                .IsRequired()
                .HasColumnName("state")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
