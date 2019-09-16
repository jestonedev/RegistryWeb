using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class LogOwnerProcessConfiguration : IEntityTypeConfiguration<LogOwnerProcess>
    {
        private string nameDatebase;

        public LogOwnerProcessConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogOwnerProcess> builder)
        {
            builder.ToTable("log_owner_processes", nameDatebase);

            builder.HasIndex(e => e.IdLogObject)
                .HasName("FK_log_owner_processes_id_log_object");

            builder.HasIndex(e => e.IdLogType)
                .HasName("FK_log_owner_processes_id_log_type");

            builder.HasIndex(e => e.IdUser)
                .HasName("FK_owner_processes_log_id_user");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date).HasColumnName("date");

            builder.Property(e => e.IdUser)
                .HasColumnName("id_user")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdLogObject)
                .HasColumnName("id_log_object")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdLogType)
                .HasColumnName("id_log_type")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdLogObjectNavigation)
                .WithMany(p => p.LogOwnerProcesses)
                .HasForeignKey(d => d.IdLogObject)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_log_owner_processes_id_log_object");

            builder.HasOne(d => d.IdLogTypeNavigation)
                .WithMany(p => p.LogOwnerProcesses)
                .HasForeignKey(d => d.IdLogType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_log_owner_processes_id_log_type");

            builder.HasOne(d => d.IdUserNavigation)
                .WithMany(p => p.LogOwnerProcesses)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_processes_log_id_user");
        }
    }
}
