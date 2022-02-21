using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class OwnerProcessConfiguration : IEntityTypeConfiguration<OwnerProcess>
    {
        private string nameDatebase;

        public OwnerProcessConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerProcess> builder)
        {
            builder.HasKey(e => e.IdProcess);

            builder.ToTable("owner_processes", nameDatebase);

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.AnnulDate).HasColumnName("annul_date");

            builder.Property(e => e.AnnulComment)
               .HasColumnName("annul_comment")
               .HasMaxLength(255)
               .IsUnicode(false);

            builder.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(65535)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
