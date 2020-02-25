using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnershipRightConfiguration : IEntityTypeConfiguration<OwnershipRight>
    {
        private string nameDatebase;

        public OwnershipRightConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnershipRight> builder)
        {
            builder.HasKey(e => e.IdOwnershipRight);

            builder.ToTable("ownership_rights", nameDatebase);

            builder.HasIndex(e => e.IdOwnershipRightType)
                .HasName("FK__ort_id_ort");

            builder.Property(e => e.IdOwnershipRight)
                .HasColumnName("id_ownership_right")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DemolishPlanDate)
                .HasColumnName("demolish_plan_date")
                .HasColumnType("date");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdOwnershipRightType)
                .HasColumnName("id_ownership_right_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .HasColumnName("number")
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(e => e.ResettlePlanDate)
                .HasColumnName("resettle_plan_date")
                .HasColumnType("date");

            builder.HasOne(d => d.OwnershipRightTypeNavigation)
                .WithMany(p => p.OwnershipRights)
                .HasForeignKey(d => d.IdOwnershipRightType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ort_id_ort");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
