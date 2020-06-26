﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ActFileConfiguration : IEntityTypeConfiguration<ActFile>
    {
        private string nameDatebase;

        public ActFileConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ActFile> builder)
        {
            builder.ToTable("act_files", nameDatebase);

            builder.HasKey(e => e.IdFile);

            builder.Property(e => e.IdFile)
                .HasColumnName("id_file")
                .HasColumnType("int(11)");

            builder.Property(e => e.OriginalName)
                .HasMaxLength(255)
                .HasColumnName("original_name")
                .IsUnicode(false);

            builder.Property(e => e.FileName)
                .HasMaxLength(4096)
                .HasColumnName("file_name")
                .IsUnicode(false);
        }
    }
}
