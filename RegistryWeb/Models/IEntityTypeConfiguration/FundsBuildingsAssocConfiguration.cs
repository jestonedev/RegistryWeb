﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class FundsBuildingsAssocConfiguration : IEntityTypeConfiguration<FundsBuildingsAssoc>
    {
        private string nameDatebase;

        public FundsBuildingsAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundsBuildingsAssoc> builder)
        {
            builder.HasKey(e => new { e.IdBuilding, e.IdFund });

            builder.ToTable("funds_buildings_assoc", nameDatebase);

            builder.HasIndex(e => e.IdFund)
                .HasName("FK_funds_buildings_assoc_funds_history_id_fund");

            builder.Property(e => e.IdBuilding)
                .HasColumnName("id_building")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdFund)
                .HasColumnName("id_fund")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdFundNavigation)
                .WithMany(p => p.FundsBuildingsAssoc)
                .HasForeignKey(d => d.IdFund)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
