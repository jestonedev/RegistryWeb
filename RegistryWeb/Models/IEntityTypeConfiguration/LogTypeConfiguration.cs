﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class LogTypeConfiguration : IEntityTypeConfiguration<LogType>
    {
        private string nameDatebase;

        public LogTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<LogType> builder)
        {
            builder.HasKey(e => e.IdLogType);

            builder.ToTable("log_types", "registry_test_1");

            builder.Property(e => e.IdLogType)
                .HasColumnName("id_log_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("log_type")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}