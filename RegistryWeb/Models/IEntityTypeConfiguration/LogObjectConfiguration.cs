using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class LogObjectConfiguration : IEntityTypeConfiguration<LogObject>
    {
        private string nameDatebase;

        public LogObjectConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogObject> builder)
        {
            builder.HasKey(e => e.IdLogObject);

            builder.ToTable("log_objects", nameDatebase);

            builder.Property(e => e.IdLogObject)
                .HasColumnName("id_log_object")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("log_object")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
