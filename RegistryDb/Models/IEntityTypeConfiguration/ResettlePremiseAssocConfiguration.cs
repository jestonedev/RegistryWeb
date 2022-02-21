using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ResettlePremiseAssocConfiguration : IEntityTypeConfiguration<ResettlePremiseAssoc>
    {
        private string nameDatebase;

        public ResettlePremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ResettlePremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdPremises, e.IdResettleInfo });
            
            builder.ToTable("resettle_premise_assoc", nameDatebase);

            builder.HasIndex(e => e.IdResettleInfo)
                .HasName("FK_resettle_premise_assoc_id_resettle_info");

            builder.HasIndex(e => e.IdPremises)
                .HasName("FK_resettle_premise_assoc_id_premises");

            builder.Property(e => e.IdPremises)
                .HasColumnName("id_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdResettleInfo)
                .HasColumnName("id_resettle_info")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.ResettleInfoNavigation)
                .WithMany(p => p.ResettlePremisesAssoc)
                .HasForeignKey(d => d.IdResettleInfo)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.PremisesNavigation)
                .WithMany(p => p.ResettlePremisesAssoc)
                .HasForeignKey(d => d.IdPremises)
                .OnDelete(DeleteBehavior.Cascade);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
