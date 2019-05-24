using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnershipRightTypesConfiguration : IEntityTypeConfiguration<OwnershipRightTypes>
    {
        private string nameDatebase;

        public OwnershipRightTypesConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnershipRightTypes> builder)
        {
            builder.HasKey(e => e.IdOwnershipRightType);

            builder.ToTable("ownership_right_types", nameDatebase);

            builder.Property(e => e.IdOwnershipRightType)
                .HasColumnName("id_ownership_right_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.OwnershipRightType)
                .IsRequired()
                .HasColumnName("ownership_right_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
