using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerReasonsConfiguration : IEntityTypeConfiguration<OwnerReasons>
    {
        private string nameDatebase;

        public OwnerReasonsConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerReasons> builder)
        {
            builder.HasKey(e => e.IdReason);

            builder.ToTable("owner_reasons", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_reasons_id_process");

            builder.HasIndex(e => e.IdReasonType)
                .HasName("FK_owner_reasons_id_reason_type");

            builder.Property(e => e.IdReason)
                .HasColumnName("id_reason")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdReasonType)
                .HasColumnName("id_reason_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.ReasonDate)
                .HasColumnName("reason_date")
                .HasColumnType("date");

            builder.Property(e => e.ReasonNumber)
                .HasColumnName("reason_number")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOwnerProcessesNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_reasons_id_process");

            builder.HasOne(d => d.IdReasonTypeNavigation)
                .WithMany(p => p.OwnerReasons)
                .HasForeignKey(d => d.IdReasonType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_reasons_id_reason_type");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
