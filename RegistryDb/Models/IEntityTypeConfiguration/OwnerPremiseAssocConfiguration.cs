using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
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

            builder.HasIndex(e => e.IdPremise)
                .HasName("FK_owner_premises_assoc_id_premise");

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_premises_assoc_id_process");

            builder.Property(e => e.IdAssoc)
                .HasColumnName("id_assoc")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdPremise)
                .HasColumnName("id_premise")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.PremiseNavigation)
                .WithMany(p => p.OwnerPremisesAssoc)
                .HasForeignKey(d => d.IdPremise)
                .HasConstraintName("FK_owner_premises_assoc_id_premise");

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.OwnerPremisesAssoc)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_owner_premises_assoc_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
