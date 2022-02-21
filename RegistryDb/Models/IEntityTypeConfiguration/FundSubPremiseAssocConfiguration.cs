using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class FundSubPremiseAssocConfiguration : IEntityTypeConfiguration<FundSubPremiseAssoc>
    {
        private string nameDatebase;

        public FundSubPremiseAssocConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundSubPremiseAssoc> builder)
        {
            builder.HasKey(e => new { e.IdSubPremises, e.IdFund });

            builder.ToTable("funds_sub_premises_assoc", nameDatebase);

            builder.HasIndex(e => e.IdSubPremises)
                .HasName("FK_funds_sub_premises_assoc_sub_premises_id_sub_premises");

            builder.Property(e => e.IdSubPremises)
                .HasColumnName("id_sub_premises")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdFund)
                .HasColumnName("id_fund")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(d => d.IdFundNavigation)
                .WithMany(p => p.FundsSubPremisesAssoc)
                .HasForeignKey(d => d.IdFund)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.IdSubPremisesNavigation)
                .WithMany(p => p.FundsSubPremisesAssoc)
                .HasForeignKey(d => d.IdSubPremises);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
