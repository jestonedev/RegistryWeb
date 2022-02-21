using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class RestrictionTypeConfiguration: IEntityTypeConfiguration<RestrictionType>
    {
        private string nameDatebase;

        public RestrictionTypeConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<RestrictionType> builder)
        {
            builder.HasKey(e => e.IdRestrictionType);

            builder.ToTable("restriction_types", nameDatebase);

            builder.Property(e => e.IdRestrictionType)
                .HasColumnName("id_restriction_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.RestrictionTypeName)
                .IsRequired()
                .HasColumnName("restriction_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
