﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class KumiAccountConfiguration : IEntityTypeConfiguration<KumiAccount>
    {
        private string nameDatebase;

        public KumiAccountConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiAccount> builder)
        {
            builder.HasKey(e => e.IdAccount);

            builder.ToTable("kumi_accounts", nameDatebase);

            builder.Property(e => e.IdAccount)
                .HasColumnName("id_account")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Account)
                .HasColumnName("account")
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.AccountGisZkh)
                .HasColumnName("account_gis_zkh")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdState)
                .HasColumnName("id_state")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.BeginDate)
                .HasColumnName("begin_date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.AnnualDate)
                .HasColumnName("annual_date")
                .HasColumnType("date");

            builder.Property(e => e.RecalcMarker)
                .HasColumnName("recalc_marker")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.Property(e => e.RecalcReason)
                .HasColumnName("recalc_reason")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.LastChargeDate)
                .HasColumnName("last_charge_date")
                .HasColumnType("date");

            builder.Property(e => e.CurrentBalanceTenancy)
                .HasColumnName("current_balance_tenancy")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.CurrentBalancePenalty)
                .HasColumnName("current_balance_penalty")
                .HasColumnType("decimal(12,2)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.State).WithMany(e => e.KumiAccounts)
                .HasForeignKey(e => e.IdState);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
