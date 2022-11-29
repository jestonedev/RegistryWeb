using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.IEntityTypeConfiguration.Tenancies
{
    public class TenancyPaymentAfter28082019Configuration : IEntityTypeConfiguration<TenancyPaymentAfter28082019>
    {
        private string nameDatebase;

        public TenancyPaymentAfter28082019Configuration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPaymentAfter28082019> builder)
        {
            builder.HasKey(e => new { e.Key });

            builder.ToTable("v_payments_coefficients_all", nameDatebase);

            builder.Property(e => e.Key)
                .HasColumnName("key");

            builder.Property(e => e.IdBuilding)
                .HasColumnType("int(11)")
                .HasColumnName("id_building");

            builder.Property(e => e.IdPremises)
                .HasColumnType("int(11)")
                .HasColumnName("id_premises");

            builder.Property(e => e.IdSubPremises)
                .HasColumnType("int(11)")
                .HasColumnName("id_sub_premises");

            builder.Property(e => e.RentArea)
                .HasColumnName("rent_area")
                .HasColumnType("double");

            builder.Property(e => e.K1)
                .HasColumnName("k1")
                .HasColumnType("decimal(7,5)");

            builder.Property(e => e.K2)
                .HasColumnName("k2")
                .HasColumnType("decimal(2,1)");

            builder.Property(e => e.K3)
                .HasColumnName("k3")
                .HasColumnType("decimal(2,1)");

            builder.Property(e => e.KC)
                .HasColumnName("kc")
                .HasColumnType("decimal(3,2)");

            builder.Property(e => e.Hb)
                .HasColumnName("Hb")
                .HasColumnType("decimal(23,5)");
        }
    }
}
