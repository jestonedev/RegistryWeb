using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnershipPremiseAssocConfiguration : IEntityTypeConfiguration<OwnershipPremiseAssoc>
    {
        private string nameDatebase;

        public OwnershipPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnershipPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdOwnershipRight });

            builder.ToTable("ownership_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdOwnershipRight)
                .HasName("FK_ownership_premises_assoc_ownership_rights_id_ownership_right");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwnershipRight)
                .HasColumnName("id_ownership_right")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdOwnershipRightNavigation)
                .WithMany(p => p.OwnershipPremisesAssoc)
                .HasForeignKey(d => d.IdOwnershipRight)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.IdPremisesNavigation)
                .WithMany(p => p.OwnershipPremisesAssoc)
                .HasForeignKey(d => d.IdPremises);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
