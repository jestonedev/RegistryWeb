using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class FundTypesConfiguration : IEntityTypeConfiguration<FundTypes>
    {
        private string nameDatebase;

        public FundTypesConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundTypes> builder)
        {
            builder.HasKey(e => e.IdFundType);

            builder.ToTable("fund_types", nameDatebase);

            builder.Property(e => e.IdFundType)
                .HasColumnName("id_fund_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.FundType)
                .IsRequired()
                .HasColumnName("fund_type")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
