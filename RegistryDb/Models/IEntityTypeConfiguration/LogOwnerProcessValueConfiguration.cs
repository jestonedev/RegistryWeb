using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class LogOwnerProcessValueConfiguration : IEntityTypeConfiguration<LogOwnerProcessValue>
    {
        private string nameDatebase;

        public LogOwnerProcessValueConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogOwnerProcessValue> builder)
        {
            builder.ToTable("log_owner_processes_value", nameDatebase);

            builder.HasIndex(e => e.IdLog)
                .HasName("FK_log_owner_processes_value_id_log");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdLog)
                .HasColumnName("id_log")
                .HasColumnType("int(11)");

            builder.Property(e => e.Field)
                .IsRequired()
                .HasColumnName("field")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Value)
                .HasColumnName("value")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdLogNavigation)
                .WithMany(p => p.LogOwnerProcessesValue)
                .HasForeignKey(d => d.IdLog)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_log_owner_processes_value_id_log");
        }
    }
}