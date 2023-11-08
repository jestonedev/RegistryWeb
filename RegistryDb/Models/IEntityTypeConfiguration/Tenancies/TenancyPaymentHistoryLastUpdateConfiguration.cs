using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyPaymentHistoryLastUpdateConfiguration : IEntityTypeConfiguration<TenancyPaymentHistoryLastUpdate>
    {
        private string nameDatebase;

        public TenancyPaymentHistoryLastUpdateConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPaymentHistoryLastUpdate> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("tenancy_payments_history_last_update", nameDatebase);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.LastUpdateDate)
                .IsRequired()
                .HasColumnName("last_update_date");
        }
    }
}
