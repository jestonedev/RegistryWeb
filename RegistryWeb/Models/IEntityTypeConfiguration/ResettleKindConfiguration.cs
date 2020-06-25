using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ResettleKindConfiguration : IEntityTypeConfiguration<ResettleKind>
    {
        private string nameDatebase;

        public ResettleKindConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettleKind> builder)
        {
            builder.HasKey(e => e.IdResettleKind);

            builder.ToTable("resettle_kinds", nameDatebase);

            builder.Property(e => e.IdResettleKind)
                .HasColumnName("id_resettle_kind")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.ResettleKindName)
                .IsRequired()
                .HasColumnName("resettle_kind")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
