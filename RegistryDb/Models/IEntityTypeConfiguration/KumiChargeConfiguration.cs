using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class KumiChargeConfiguration : IEntityTypeConfiguration<KumiCharge>
    {
        private string nameDatebase;

        public KumiChargeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiCharge> builder)
        {
            builder.HasKey(e => e.IdCharge);

            builder.ToTable("kumi_charges", nameDatebase);

            builder.Property(e => e.IdCharge)
                .HasColumnName("id_charge")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.InputTenancy)
                .HasColumnName("input_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.InputPenalty)
                .HasColumnName("input_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargeTenancy)
                .HasColumnName("charge_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.ChargePenalty)
                .HasColumnName("charge_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentTenancy)
                .HasColumnName("payment_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.PaymentPenalty)
                .HasColumnName("payment_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcTenancy)
                .HasColumnName("recalc_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.RecalcPenalty)
                .HasColumnName("recalc_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputTenancy)
                .HasColumnName("output_tenancy")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.OutputPenalty)
                .HasColumnName("output_penalty")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.Account).WithMany(e => e.Charges)
                .HasForeignKey(e => e.IdAccount);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
