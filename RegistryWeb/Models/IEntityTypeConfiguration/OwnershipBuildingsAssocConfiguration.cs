using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnershipBuildingsAssocConfiguration : IEntityTypeConfiguration<OwnershipBuildingsAssoc>
    {
        private string nameDatebase;

        public OwnershipBuildingsAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnershipBuildingsAssoc> builder)
        {
            builder.HasKey(e => new { e.IdBuilding, e.IdOwnershipRight });

            builder.ToTable("ownership_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdOwnershipRight)
                .HasName("FK_ownership_buildings_assoc_ownership_rights_id_ownership_right");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnershipRight)
                .HasColumnName("id_ownership_right")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOwnershipRightNavigation)
                .WithMany(p => p.OwnershipBuildingsAssoc)
                .HasForeignKey(d => d.IdOwnershipRight)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
