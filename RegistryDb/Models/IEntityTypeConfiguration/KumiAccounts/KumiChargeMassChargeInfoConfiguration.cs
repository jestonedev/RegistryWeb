using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiChargeMassCalcInfoConfiguration : IEntityTypeConfiguration<KumiChargeMassCalcInfo>
    {
        private string nameDatebase;

        public KumiChargeMassCalcInfoConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiChargeMassCalcInfo> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("kumi_charges_masscalcinfo", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.LastCalcDate)
                .HasColumnName("last_calc_date")
                .HasColumnType("date")
                .IsRequired();
        }
    }
}
