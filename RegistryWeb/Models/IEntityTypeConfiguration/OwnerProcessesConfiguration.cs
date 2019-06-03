using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerProcessesConfiguration : IEntityTypeConfiguration<OwnerProcesses>
    {
        private string nameDatebase;

        public OwnerProcessesConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerProcesses> builder)
        {
            builder.HasKey(e => e.IdProcess);

            builder.ToTable("owner_processes", nameDatebase);

            builder.HasIndex(e => e.IdOwnerType)
                .HasName("FK_owner_processes_owner_type_id_owner_type");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnerType)
                .HasColumnName("id_owner_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.AnnulDate).HasColumnName("annul_date");

            builder.Property(e => e.FromDate).HasColumnName("from_date");

            builder.Property(e => e.ToDate).HasColumnName("to_date");

            builder.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(65535)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOwnerTypeNavigation)
                .WithMany(p => p.OwnerProcesses)
                .HasForeignKey(d => d.IdOwnerType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
