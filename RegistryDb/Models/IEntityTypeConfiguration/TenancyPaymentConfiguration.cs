using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.SqlViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class TenancyPaymentConfiguration : IEntityTypeConfiguration<TenancyPayment>
    {
        private string nameDatebase;

        public TenancyPaymentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPayment> builder)
        {
            builder.HasKey(e => new { e.Key });

            builder.ToTable("v_rent_objects_payment", nameDatebase);

            builder.Property(e => e.Key)
                .HasColumnName("key");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises");

            builder.Property(e => e.RentArea)
                .HasColumnName("rent_area")
                .HasColumnType("double");

            builder.Property(e => e.IdRentCategory)
                .HasColumnName("id_rent_category")
                .HasColumnType("int(11)");

            builder.Property(e => e.Payment)
                .HasColumnName("payment")
                .HasColumnType("decimal(18,2)");
        }
    }
}
