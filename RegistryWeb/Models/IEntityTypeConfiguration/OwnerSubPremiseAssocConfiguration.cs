using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerSubPremiseAssocConfiguration : IEntityTypeConfiguration<OwnerSubPremiseAssoc>
    {
        private string nameDatebase;

        public OwnerSubPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerSubPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("owner_sub_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_sub_premises_assoc_id_process");

            builder.HasIndex(e => e.IdSubPremises)
                .HasName("FK_owner_sub_premises_assoc_id_sub_premises");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.OwnerSubPremisesAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_sub_premises_assoc_id_process");

            builder.HasOne(d => d.IdSubPremisesNavigation)
                .WithMany(p => p.OwnerSubPremisesAssoc)
                .HasForeignKey(d => d.IdSubPremises)
                .HasConstraintName("FK_owner_sub_premises_assoc_id_sub_premises");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
