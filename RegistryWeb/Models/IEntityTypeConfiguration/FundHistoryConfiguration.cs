using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class FundHistoryConfiguration : IEntityTypeConfiguration<FundHistory>
    {
        private string nameDatebase;

        public FundHistoryConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<FundHistory> builder)
        {
            builder.HasKey(e => e.IdFund);

            builder.ToTable("funds_history", nameDatebase);

            builder.HasIndex(e => e.IdFundType)
                .HasName("FK_funds_history_fund_types_id_fund_type");

            builder.Property(e => e.IdFund)
                .HasColumnName("id_fund")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsUnicode(false);

            builder.Property(e => e.ExcludeRestrictionDate).HasColumnName("exclude_restriction_date");

            builder.Property(e => e.ExcludeRestrictionDescription)
                .HasColumnName("exclude_restriction_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ExcludeRestrictionNumber)
                .HasColumnName("exclude_restriction_number")
                .HasMaxLength(30)
                .IsUnicode(false);

            builder.Property(e => e.IdFundType)
                .HasColumnName("id_fund_type")
                .HasColumnType("int(11)")
                .HasDefaultValueSql("1");

            builder.Property(e => e.IncludeRestrictionDate).HasColumnName("include_restriction_date");

            builder.Property(e => e.IncludeRestrictionDescription)
                .HasColumnName("include_restriction_description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IncludeRestrictionNumber)
                .HasColumnName("include_restriction_number")
                .HasMaxLength(30)
                .IsUnicode(false);

            builder.Property(e => e.ProtocolDate)
                .HasColumnName("protocol_date")
                .HasColumnType("date");

            builder.Property(e => e.ProtocolNumber)
                .HasColumnName("protocol_number")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.HasOne(d => d.IdFundTypeNavigation)
                .WithMany(p => p.FundsHistory)
                .HasForeignKey(d => d.IdFundType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
