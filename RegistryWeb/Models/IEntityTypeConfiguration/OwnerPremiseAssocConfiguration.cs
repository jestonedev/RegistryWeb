using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerPremiseAssocConfiguration : IEntityTypeConfiguration<OwnerPremiseAssoc>
    {
        private string nameDatebase;

        public OwnerPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerPremiseAssoc> builder)
        {
            builder.HasKey(e => e.IdAssoc);

            builder.ToTable("owner_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdPremises)
                .HasName("FK_owner_premises_assoc_id_premises");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_premises_assoc_id_process");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdPremisesNavigation)
                .WithMany(p => p.OwnerPremisesAssoc)
                .HasForeignKey(d => d.IdPremises)
                .HasConstraintName("FK_owner_premises_assoc_id_premises");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.OwnerPremisesAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_premises_assoc_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
