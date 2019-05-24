using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerBuildingsAssocConfiguration : IEntityTypeConfiguration<OwnerBuildingsAssoc>
    {
        private string nameDatebase;

        public OwnerBuildingsAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerBuildingsAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("owner_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdBuilding)
                .HasName("FK_owner_buildings_assoc_id_building");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_buildings_assoc_id_process");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.OwnerBuildingsAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_buildings_assoc_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
