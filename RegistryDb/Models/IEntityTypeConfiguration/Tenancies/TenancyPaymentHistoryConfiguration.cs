using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyPaymentHistoryConfiguration : IEntityTypeConfiguration<TenancyPaymentHistory>
    {
        private string nameDatebase;

        public TenancyPaymentHistoryConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPaymentHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("tenancy_payments_history", nameDatebase);

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.RentArea)
                .HasColumnName("rent_area")
                .HasColumnType("double")
                .IsRequired();

            builder.Property(e => e.K1)
                .HasColumnName("k1")
                .HasColumnType("decimal(7,5)")
                .IsRequired();

            builder.Property(e => e.K2)
                .HasColumnName("k2")
                .HasColumnType("decimal(2,1)")
                .IsRequired();

            builder.Property(e => e.K3)
                .HasColumnName("k3")
                .HasColumnType("decimal(2,1)")
                .IsRequired();

            builder.Property(e => e.Kc)
                .HasColumnName("kc")
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            builder.Property(e => e.Hb)
                .HasColumnName("Hb")
                .HasColumnType("decimal(23,5)")
                .IsRequired();

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.Reason)
                .IsRequired()
                .HasColumnName("reason")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.HasOne(d => d.Building)
               .WithMany(p => p.TenancyPaymentsHistory)
               .HasForeignKey(d => d.IdBuilding)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_tenancy_payments_history_id_building");

            builder.HasOne(d => d.Premise)
                .WithMany(p => p.TenancyPaymentsHistory)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tenancy_payments_history_id_premises");

            builder.HasOne(d => d.SubPremise)
                .WithMany(p => p.TenancyPaymentsHistory)
                .HasForeignKey(d => d.IdSubPremises)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tenancy_payments_history_id_sub_premises");
        }
    }
}
